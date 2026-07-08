using BusBooking.Entity;
using BusBooking.Models;
using BusBooking.Repository;
using BusBooking.Repository.IRepositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BusBooking.Controllers
{
    public class UsersController(IUnitOfWork unitOfWork, IConfiguration configuration, ILogger<UsersController> logger) : Controller
    {
        //private readonly IBookingRepository _bookingRepository;
        private readonly IConfiguration _configuration = configuration;
        private readonly ILogger<UsersController> _logger = logger;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        //private readonly ISeatRepository _seatRepository;
        //private readonly IUserRepository _userRepository;
        //private readonly ITripRepository _TripRepository;

        public object? HashHelper { get; private set; }

        public IActionResult Index()
        {
            var users = _unitOfWork.User.GetAll().ToList();
            return View(users);
        }

        public IActionResult SignUp()
        {

            return View();
        }

        [HttpPost]
        public IActionResult SignUp(SignUpVM model)
        {
            if (ModelState.IsValid)
            {
                var user = _unitOfWork.User.Get(x => x.Username == model.Username || x.Email == model.Email);
                if (user == null)
                {
                    var hashedPassword = _unitOfWork.User.HashPassword(model.Password);
                    _unitOfWork.User.Add(new User
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Username = model.Username,
                        Phone = model.Phone,
                        Email = model.Email,
                        PasswordHash = hashedPassword,
                        Role = "Customer",
                    });
                    _unitOfWork.Save();

                    return RedirectToAction("SignIn");
                }
                else
                {
                    ModelState.AddModelError("", "Email and password already exist");
                }
            }
            return View(model);
        }

        public IActionResult SignIn()
        {
            var model = new SignInVM();
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Trip");
            }

            return View(model);
        }

        // POST: Trips/Login
        [HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public IActionResult SignIn(SignInVM model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var user = _unitOfWork.User.Get(x => x.Email == model.Email);
        //        if (user != null && _unitOfWork.User.VerifyPassword(model.Password, user.Password))
        //        {
        //            // Проверка на администратора
        //            var isAdmin = (user.Email == "xyz@mail.com");

        //            var claims = new List<Claim>
        //    {
        //        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        //        new Claim(ClaimTypes.Name, user.Username),
        //        new Claim(ClaimTypes.Email, user.Email),
        //        new Claim(ClaimTypes.Role, isAdmin ? "admin" : "user")
        //    };

        //            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        //            var principal = new ClaimsPrincipal(identity);
        //            HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal).Wait();

        //            return RedirectToAction("Index", "Trip"); // Перенаправить на главную страницу
        //        }
        //        else
        //        {
        //            ModelState.AddModelError("", "Invalid login attempt.");
        //            return View(model); // Вернуть форму с ошибкой
        //        }
        //    }

        //    return View(model); // Вернуть форму с ошибками валидации
        //}

        public async Task<IActionResult> SignIn(SignInVM model)
        {
            if (ModelState.IsValid)
            {
                var user = _unitOfWork.User.Get(x => x.Email == model.Email);
                if (user != null && _unitOfWork.User.VerifyPassword(model.Password, user.PasswordHash)) // Добавлена проверка пароля
                {
                    bool isAdmin = (user.Email == "xyz@mail.com");

                    var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Username),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, isAdmin ? "admin" : "user")
            };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = true
                    };

                    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties
                    );

                    return RedirectToAction("Index", "Trip");
                }

                ModelState.AddModelError("", "Email or password is incorrect");
            }

            return View(model);
        }

        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("SignIn");
        }
    }
}
