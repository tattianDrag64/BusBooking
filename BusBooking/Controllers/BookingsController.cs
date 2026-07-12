using BusBooking.Entity;
using BusBooking.Models;
using BusBooking.Repository.UnitOfWork;
using BusBooking.Services.ServiceManager;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using System.Security.Claims;

namespace BusBooking.Controllers
{
    [ApiController]
    [Route("api/bookings")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class BookingsController(IUnitOfWork unitOfWork, IServiceManager services) : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IServiceManager _services = services;

        // GET api/bookings/my
        [HttpGet("my")]
        public IActionResult MyBookings()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userId = Guid.Parse(userIdClaim.Value);
            var orders = _unitOfWork.Order.GetOrdersByUser(userId).Select(o => new OrderSummaryVM
            {
                Id = o.Id,
                OrderCode = o.OrderCode,
                From = o.Trip.From,
                To = o.Trip.To,
                DepartureDate = o.Trip.DepartureDate,
                TotalPrice = o.TotalPrice,
                CreatedAt = o.CreatedAt,
                SeatNumbers = o.OrderSeats.Select(os => os.SeatDetail.SeatNumber),
                Status = o.Status
            });

            return Ok(orders);
        }

        // GET api/bookings — admin listing across all users (MyBookings above is scoped to
        // the caller only, not suitable for the admin panel).
        [HttpGet]
        [Authorize(Roles = nameof(UserRole.Admin), AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult AllBookings()
        {
            var orders = _unitOfWork.Order.GetAllWithDetails().Select(o => new AdminOrderSummaryVM
            {
                Id = o.Id,
                OrderCode = o.OrderCode,
                UserEmail = o.User.Email,
                From = o.Trip.From,
                To = o.Trip.To,
                DepartureDate = o.Trip.DepartureDate,
                TotalPrice = o.TotalPrice,
                CreatedAt = o.CreatedAt,
                SeatNumbers = o.OrderSeats.Select(os => os.SeatDetail.SeatNumber),
                Status = o.Status
            });

            return Ok(orders);
        }

        // POST api/bookings/{orderId}/cancel — the owning customer cancels their own booking.
        [HttpPost("{orderId}/cancel")]
        public IActionResult Cancel(Guid orderId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }
            var userId = Guid.Parse(userIdClaim.Value);

            var order = _unitOfWork.Order.GetByIdWithDetails(orderId);
            if (order == null || order.UserId != userId)
            {
                return NotFound(new { message = "Order not found." });
            }
            if (order.Trip.DepartureDate <= DateTime.UtcNow)
            {
                return Conflict(new { message = "This trip has already departed — it can no longer be cancelled." });
            }

            return CancelOrder(order, reason: null);
        }

        // POST api/bookings/{orderId}/admin-cancel — admin cancels any booking, with a reason.
        [HttpPost("{orderId}/admin-cancel")]
        [Authorize(Roles = nameof(UserRole.Admin), AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult AdminCancel(Guid orderId, [FromBody] AdminCancelOrderVM model)
        {
            var order = _unitOfWork.Order.GetByIdWithDetails(orderId);
            if (order == null)
            {
                return NotFound(new { message = "Order not found." });
            }

            return CancelOrder(order, model.Reason);
        }

        // Shared by both cancel endpoints — refunds a paid order via Stripe, or just cancels
        // an unpaid one, then always releases the booked seats back to the pool.
        private IActionResult CancelOrder(Order order, string? reason)
        {
            if (order.Status is OrderStatus.Cancelled or OrderStatus.Refunded or OrderStatus.Expired)
            {
                return Conflict(new { message = $"Order is already {order.Status} — cannot cancel." });
            }

            if (order.Status == OrderStatus.Confirmed)
            {
                if (string.IsNullOrEmpty(order.StripePaymentIntentId))
                {
                    return Conflict(new { message = "Cannot refund — no payment record for this order." });
                }
                try
                {
                    _services.PaymentService.RefundPayment(order.StripePaymentIntentId);
                }
                catch (StripeException)
                {
                    return StatusCode(StatusCodes.Status502BadGateway,
                        new { message = "Payment provider is unavailable right now. Please try again shortly." });
                }
                order.Status = OrderStatus.Refunded;
            }
            else
            {
                order.Status = OrderStatus.Cancelled;
            }

            order.CancellationReason = reason;
            _unitOfWork.Order.Update(order);

            var seatIds = order.OrderSeats.Select(os => os.SeatDetailId).ToList();
            _unitOfWork.SeatDetail.ReleaseSeats(seatIds);
            _unitOfWork.OrderSeat.RemoveRange(order.OrderSeats);

            _unitOfWork.Save();

            return NoContent();
        }
    }
}
