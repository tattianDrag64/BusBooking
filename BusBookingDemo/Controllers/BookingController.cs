using BusBookingDemo.Entity;
using BusBookingDemo.Models;
using BusBookingDemo.Repository.IRepositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BusBookingDemo.Controllers
{
    public class BookingController : Controller
    {
        protected readonly IUnitOfWork UnitOfWork;

        public BookingController(IUnitOfWork unitOfWork)
        {
            UnitOfWork = unitOfWork;
        }
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

            var userId = int.Parse(userIdClaim.Value); // Claim'den alınan değeri int'e çevir
            // Kullanıcının rezervasyonlarını repository'den al ve ilişkili Trip ve Seat bilgilerini yükle
            IQueryable<Booking> bookingsList = UnitOfWork.Booking.GetBookingsByUserId(userId);


            // ViewModel'i oluştur ve rezervasyon listesini ata
            var viewModel = new BookingVM
            {
                Bookings = bookingsList
            };

            // ViewModel'i görünüme gönder
            return View(viewModel);
        }
    }
}
