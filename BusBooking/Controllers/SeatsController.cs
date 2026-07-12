using BusBooking.Entity;
using BusBooking.Models;
using BusBooking.Repository.UnitOfWork;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BusBooking.Controllers
{
    [ApiController]
    [Route("api/seats")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class SeatsController(IUnitOfWork unitOfWork) : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        private static readonly TimeSpan SeatHoldDuration = TimeSpan.FromMinutes(15);

        // GET api/seats/trip/{tripId}
        [HttpGet("trip/{tripId}")]
        public IActionResult GetSeatsForTrip(Guid tripId)
        {
            var trip = _unitOfWork.Trip.GetById(tripId);
            if (trip == null)
            {
                return NotFound(new { message = "Trip not found." });
            }

            var seats = _unitOfWork.SeatDetail.GetAll(s => s.TripId == tripId).ToList();
            if (seats.Count == 0)
            {
                var seatsCount = _unitOfWork.Bus.GetSeatsCount(trip.BusId);
                for (int i = 1; i <= seatsCount; i++)
                {
                    seats.Add(new SeatDetail { TripId = tripId, SeatNumber = $"Seat {i}", IsOccupied = false });
                }
                _unitOfWork.SeatDetail.AddRange(seats);
                _unitOfWork.Save();
            }
            else
            {
                ExpireStaleHolds(tripId);
                seats = _unitOfWork.SeatDetail.GetAll(s => s.TripId == tripId).ToList();
            }

            var now = DateTime.UtcNow;
            var result = seats.Select(s => new SeatDetailVM
            {
                Id = s.Id,
                SeatNumber = s.SeatNumber,
                // Unavailable = permanently booked, or actively held by someone mid-checkout.
                IsOccupied = s.IsOccupied || (s.IsReserved && s.ReservedUntil > now)
            });

            return Ok(result);
        }

        // POST api/seats/book
        [HttpPost("book")]
        public IActionResult BookSeat([FromBody] BookSeatRequestVM model)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var seatIds = model.SeatIds.Distinct().ToList();
            if (seatIds.Count == 0)
            {
                return BadRequest(new { message = "Select at least one seat." });
            }

            var trip = _unitOfWork.Trip.GetById(model.TripId);
            if (trip == null)
            {
                return NotFound(new { message = "Trip not found." });
            }

            var seatsOnTrip = _unitOfWork.SeatDetail
                .GetAll(s => seatIds.Contains(s.Id) && s.TripId == model.TripId)
                .Select(s => s.Id)
                .ToHashSet();
            if (seatIds.Any(id => !seatsOnTrip.Contains(id)))
            {
                return NotFound(new { message = "One or more seats not found for this trip." });
            }

            // Booking only places a hold — seats become permanently occupied once the
            // payment webhook confirms the Stripe Checkout session (see PaymentsController).
            // All-or-nothing: if any seat is taken, none of them are held for this order.
            var unavailable = _unitOfWork.SeatDetail.HoldSeats(seatIds, SeatHoldDuration).ToList();
            if (unavailable.Count > 0)
            {
                return Conflict(new
                {
                    message = "One or more seats are already booked or held by someone else.",
                    seatIds = unavailable,
                });
            }

            var userId = Guid.Parse(userIdClaim.Value);
            var holdExpiresAt = DateTime.UtcNow.Add(SeatHoldDuration);
            var order = new Order
            {
                Id = Guid.NewGuid(),
                OrderCode = "ORD-" + Guid.NewGuid().ToString("N")[..8].ToUpper(),
                UserId = userId,
                RouteId = trip.RouteId,
                TripId = model.TripId,
                TotalPrice = (decimal)trip.Price * seatIds.Count,
                Status = OrderStatus.PendingPayment
            };
            _unitOfWork.Order.Add(order);

            _unitOfWork.OrderSeat.AddRange(seatIds.Select(seatId => new OrderSeat
            {
                OrderId = order.Id,
                SeatDetailId = seatId,
                TripId = model.TripId,
            }));

            _unitOfWork.Save();

            return Ok(new
            {
                orderId = order.Id,
                orderCode = order.OrderCode,
                seatIds,
                holdExpiresAt,
            });
        }

        // Releases seat holds whose TTL passed and marks their still-pending orders as Expired.
        // Checked lazily on read rather than via a background job — acceptable for MVP scale.
        private void ExpireStaleHolds(Guid tripId)
        {
            var releasedSeatIds = _unitOfWork.SeatDetail.ReleaseExpiredHolds(tripId).ToList();
            if (releasedSeatIds.Count == 0)
            {
                return;
            }

            var orderSeats = _unitOfWork.OrderSeat.GetBySeatDetailIds(releasedSeatIds).ToList();
            if (orderSeats.Count == 0)
            {
                return;
            }

            var orderIds = orderSeats.Select(os => os.OrderId).Distinct();
            var orders = _unitOfWork.Order.GetByIds(orderIds, tracked: true);

            foreach (var order in orders.Where(o => o.Status == OrderStatus.PendingPayment))
            {
                order.Status = OrderStatus.Expired;
            }

            // Delete the OrderSeat rows, not just flip the order's status — OrderSeat.SeatDetailId
            // has a unique index, so leaving the row behind would permanently block rebooking this
            // seat after any expired hold (see ROADMAP.md, Фаза 5).
            _unitOfWork.OrderSeat.RemoveRange(orderSeats);

            _unitOfWork.Save();
        }
    }
}
