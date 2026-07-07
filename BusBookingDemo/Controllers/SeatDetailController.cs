using BusBookingDemo.Entity;
using BusBookingDemo.Models;
using BusBookingDemo.Repository.IRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BusBookingDemo.Controllers
{
    public class SeatDetailController
    {
        public class SeatController : Controller
        {
            protected readonly IUnitOfWork UnitOfWork;
            private readonly IBookingRepository _bookingRepository;
            private readonly ILogger<SeatController> _logger;
            //private readonly DataContext _context;

            public SeatController(IUnitOfWork unitOfWork, IBookingRepository bookingRepository, ILogger<SeatController> logger)
            {
                UnitOfWork = unitOfWork;
                
                _bookingRepository = bookingRepository;
                _logger = logger;
            }

            public IActionResult Index()
            {
                return View();
            }

            //[Authorize]
            public IActionResult ChooseSeats(int tripId)
            {
                var tripExists = UnitOfWork.Trip.Get(f => f.Id == tripId);
                if (tripExists == null)
                {
                    // Если рейс не найден, возвращаем 404
                    return NotFound(tripId);
                }

                var seats = UnitOfWork.SeatDetail.GetAll(s => s.TripId == tripId).ToList();
                // Если нет мест, создаём 20 мест по умолчанию
                if (!seats.Any())
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
                    Seats = seats.Select(s => new SeatDetailVM
                    {
                        Id = s.Id,
                        SeatNumber = s.SeatNumber,
                        IsOccupied = s.IsOccupied
                    }).ToList()
                };

                return View(model);
            }

            //[Authorize]
            [HttpPost]
            public IActionResult ChooseSeats(SeatBookingVM model, int tripId)
            {
                // Резервируем выбранное место
                UnitOfWork.SeatDetail.ReserveSeat(model.SelectedSeat);
                TempData["SuccessMessage"] = "Ваш полет был успешно забронирован.";

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null)
                {
                    var userId = int.Parse(userIdClaim.Value);
                    var booking = new Booking
                    {
                        UserId = userId,
                        TripId = tripId,
                        SeatDetailId = model.SelectedSeat,
                    };

                    UnitOfWork.Booking.Add(booking);
                    UnitOfWork.Save();

                    return RedirectToAction("Index", "Trip");
                }
                else
                {
                    return View("Index");
                }
            }
        }

    }
}
