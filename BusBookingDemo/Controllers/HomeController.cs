using System.Diagnostics;
using BusBookingDemo.Entity;
using BusBookingDemo.Models;
using BusBookingDemo.Repository;
using BusBookingDemo.Repository.IRepositories;
using Microsoft.AspNetCore.Mvc;
using Mono.TextTemplating;
using NuGet.Protocol.Core.Types;

namespace BusBookingDemo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
