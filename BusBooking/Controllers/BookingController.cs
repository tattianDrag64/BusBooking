using BusBooking.Models;
using BusBooking.Repository.UnitOfWork;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BusBooking.Controllers
{
    public class BookingController(IUnitOfWork unitOfWork) : Controller
    {
        protected readonly IUnitOfWork UnitOfWork = unitOfWork;

        public IActionResult MyBookings()
        {
            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return RedirectToAction("Index", "Trip");
            }

            var userId = Guid.Parse(userIdClaim.Value);
            var ordersList = UnitOfWork.Order.GetOrdersByUser(userId);
            var viewModel = new OrderVM
            {
                Orders = ordersList
            };
            return View(viewModel);
        }
    }
}
