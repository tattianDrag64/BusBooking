using BusBookingDemo.Entity;
using BusBookingDemo.Models;
using BusBookingDemo.Repository.IRepositories;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BusBookingDemo.Controllers
{
    public class BookingController(IUnitOfWork unitOfWork) : Controller
    {
        protected readonly IUnitOfWork UnitOfWork = unitOfWork;

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult MyBookings()
        {
            // Oturumda giriş yapmış kullanıcının ID'sini al
            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                // Kullanıcı giriş yapmamışsa veya ID claim'i bulunamıyorsa, hata döndür veya uygun bir sayfaya yönlendir
                return RedirectToAction("Index", "Trip"); // Örnek bir yönlendirme
            }

            var userId = Guid.Parse(userIdClaim.Value);
            // Kullanıcının rezervasyonlarını repository'den al ve ilişkili Trip ve Seat bilgilerini yükle
            var ordersList = UnitOfWork.Order.GetOrdersByUser(userId);

            // ViewModel'i oluştur ve rezervasyon listesini ata
            var viewModel = new OrderVM
            {
                Orders = ordersList
            };

            // ViewModel'i görünüme gönder
            return View(viewModel);
        }
    }
}
