using BusBookingDemo.Entity;
using BusBookingDemo.Models;
using BusBookingDemo.Repository.IRepositories;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BusBookingDemo.Controllers
{
    public class SeatDetailController
    {
        public class SeatController(IUnitOfWork unitOfWork, ILogger<SeatController> logger) : Controller
        {
            protected readonly IUnitOfWork UnitOfWork = unitOfWork;
            private readonly ILogger<SeatController> _logger = logger;

            public IActionResult Index()
            {
                return View();
            }

            //[Authorize]
            public IActionResult ChooseSeats(Guid tripId)
            {
                var tripExists = UnitOfWork.Trip.Get(f => f.Id == tripId);
                if (tripExists == null)
                {
                    // Если рейс не найден, возвращаем 404
                    return NotFound(tripId);
                }

                var seats = UnitOfWork.SeatDetail.GetAll(s => s.TripId == tripId).ToList();
                // Если нет мест, создаём 20 мест по умолчанию
                if (seats.Count == 0)
                {
                    var busId = tripExists.BusId;
                    var seatsCount = UnitOfWork.Bus.GetSeatsCount(busId);
                    for (int i = 1; i <= seatsCount; i++)
                    {
                        seats.Add(new SeatDetail { TripId = tripId, SeatNumber = $"Seat {i}", IsOccupied = false });
                    }
                    UnitOfWork.SeatDetail.AddRange(seats);
                    UnitOfWork.Save();
                }

                var model = new SeatBookingVM
                {
                    TripId = tripId,
                    Seats = [.. seats.Select(s => new SeatDetailVM
                    {
                        Id = s.Id,
                        SeatNumber = s.SeatNumber,
                        IsOccupied = s.IsOccupied
                    })]
                };

                return View(model);
            }

            //[Authorize]
            [HttpPost]
            public IActionResult ChooseSeats(SeatBookingVM model, Guid tripId)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return View("Index");
                }

                var trip = UnitOfWork.Trip.Get(t => t.Id == tripId);
                if (trip == null)
                {
                    return NotFound(tripId);
                }

                // Резервируем выбранное место
                UnitOfWork.SeatDetail.ReserveSeat(model.SelectedSeat);
                TempData["SuccessMessage"] = "Ваш рейс был успешно забронирован.";

                var userId = Guid.Parse(userIdClaim.Value);
                var order = new Order
                {
                    Id = Guid.NewGuid(),
                    OrderCode = "ORD-" + Guid.NewGuid().ToString("N")[..8].ToUpper(),
                    UserId = userId,
                    RouteId = trip.RouteId,
                    TripId = tripId,
                    TotalPrice = (decimal)trip.Price
                };
                UnitOfWork.Order.Add(order);

                var orderSeat = new OrderSeat
                {
                    OrderId = order.Id,
                    SeatDetailId = model.SelectedSeat,
                    TripId = tripId
                };
                UnitOfWork.OrderSeat.Add(orderSeat);

                UnitOfWork.Save();

                return RedirectToAction("Index", "Trip");
            }
        }

    }
}
