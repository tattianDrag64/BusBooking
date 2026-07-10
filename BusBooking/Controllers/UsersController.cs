using BusBooking.Entity;
using BusBooking.Models;
using BusBooking.Repository.UnitOfWork;
using BusBooking.Services.ServiceManager;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BusBooking.Controllers
{
    public class UsersController(IUnitOfWork unitOfWork, IServiceManager services, IConfiguration configuration, ILogger<UsersController> logger) : Controller
    {
        private readonly IConfiguration _configuration = configuration;
        private readonly ILogger<UsersController> _logger = logger;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IServiceManager _services = services;

        public object? HashHelper { get; private set; }

        [Authorize(Roles = "admin")]
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
                var user = _unitOfWork.User.Get(x => x.Username.ToLower() == model.Username.ToLower() || x.Email.ToLower() == model.Email.ToLower());
                if (user == null)
                {
                    var hashedPassword = _services.PasswordHasherService.HashPassword(model.Password);
                    _unitOfWork.User.Add(new User
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Username = model.Username,
                        Phone = model.Phone,
                        Email = model.Email,
                        PasswordHash = hashedPassword,
                        Role = UserRole.Customer,
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


        [HttpPost]
        public async Task<IActionResult> SignIn(SignInVM model)
        {
            if (ModelState.IsValid)
            {
                var user = _unitOfWork.User.Get(x => x.Email.ToLower() == model.Email.ToLower());
                if (user != null && _services.PasswordHasherService.VerifyPassword(model.Password, user.PasswordHash)) // Добавлена проверка пароля
                {
                    bool isAdmin = user.Role == UserRole.Admin;

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

        public async Task<IActionResult> AwaitLogout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("SignIn");
        }
    }
}
