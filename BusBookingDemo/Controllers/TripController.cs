using Azure.Core;
using Azure;
using BusBookingDemo.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BusBookingDemo.Repository.IRepositories;
using BusBookingDemo.Repository;
using BusBookingDemo.Models;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Core.Types;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Rendering;
using Mono.TextTemplating;

namespace BusBookingDemo.Controllers
{
    public class TripController : Controller
    {
        private IUnitOfWork _unitOfWork;
        private readonly ILogger<TripRepository> _logger;
        public TripController(IUnitOfWork unitOfWork, ILogger<TripRepository> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        public IActionResult Index()
        {
           
            return View();
        }
        [HttpPost]
        public IActionResult Index(TripVM model)
        {
            if (ModelState.IsValid)
            {
                // Поиск поездки
                var trip = _unitOfWork.Trip.Get(x => x.From == model.From && x.To == model.To);

                if (trip == null)
                {
                    // Создание новой поездки
                    var newTrip = new Trip
                    {
                        From = model.From,
                        To = model.To,
                        Depart = model.Depart,
                        Return = model.Return,
                        Guest = model.Guest
                    };
                    _unitOfWork.Trip.Add(newTrip);
                    _unitOfWork.Save();

                    // Добавление в репозиторий


                    // Перенаправление на Index
                    return RedirectToAction("SignIn");
                }
                
            }
            return View(model);
        }

        //[Authorize(Roles = "admin")]
        public IActionResult Create()
        {
            var model = new TripCreateVM
            {
                BusList = _unitOfWork.Bus.GetAll().Select(u => new SelectListItem
                {
                    Text = u.BusNumber,
                    Value = u.Id.ToString()
                })// Приводим к списку, чтобы избежать проблем с ленивой загрузкой
            };
            return View(model);
        }

        [HttpPost]
        //[Authorize(Roles = "admin")]

        public IActionResult Create(TripCreateVM model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Получаем идентификатор пользователя

            if (ModelState.IsValid)
            {
                // Преобразуем данные из модели представления в сущность Trip
                var newTrip = new Trip
                {
                    From = model.From,
                    To = model.To,
                    Depart = Convert.ToString(model.Depart),
                    Return = model.Return,
                    Time = model.Time,
                    Guest = model.Guest
                };
                model.BusList = _unitOfWork.Bus.GetAll().Select(u => new SelectListItem
            {
                Text = u.BusNumber,
                Value = u.Id.ToString()
            });

                // Добавляем новый объект в базу данных
                _unitOfWork.Trip.Add(newTrip);
                _unitOfWork.Save();

                return RedirectToAction("ListTrips"); // Перенаправляем на список поездок
            }

            // Если валидация не прошла, заново создаем модель для представления
            

            return View(model); // Возвращаем представление с текущей моделью
        }


        public IActionResult SearchTrips(TripVM model)
        {

            if (!User.Identity.IsAuthenticated)
            {
                // TempData kullanarak bir sonraki request'e kadar veri saklayabilirsiniz.
                TempData["Error"] = "You must be logged in to view this page.";
                return RedirectToAction("Index");
            }
            if (string.IsNullOrWhiteSpace(model.Depart))
            {
                //ModelState.AddModelError("", "Departure location cannot be empty.");
                TempData["Error"] = "Departure location cannot be empty.";
                return RedirectToAction("Index");
            }
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }
            var trips = _unitOfWork.Trip.GetAll().Where(f => f.From == model.From && f.To == model.To && f.Guest == model.Guest);

            // Фильтрация по One Way или Return
            if (model.IsOneWay)
            {
                trips = trips.Where(f => f.Depart == model.Depart);
            }
            else
            {
                trips = trips.Where(f => f.Depart == model.Depart &&
                                              (string.IsNullOrEmpty(model.Return) || f.Return == model.Return));
            }

            // Преобразование данных в представление
            var tripResults = trips.Select(f => new TripCreateVM
            {
                From = f.From,
                To = f.To,
                Depart = f.Depart,
                Return = f.Return,
                Time = f.Time,
                Guest = f.Guest
            }).ToList();

            return View("SearchTrips", tripResults);
        }






        [Authorize(Roles = "admin")]
        public IActionResult ListTrips()
        {
            var trips = _unitOfWork.Trip.GetAll()
                .Select(tripsEntity => new TripCreateVM
                {
                    //Id = TripEntity.Id,
                    From = tripsEntity.From,
                    To = tripsEntity.To,
                    Time = tripsEntity.Time
                    // Burada diğer gerekli alanları da ekleyebilirsiniz
                });

            return View("SearchTrips", trips);
        }


        public IActionResult Edit(int id)
        {
            var trip = _unitOfWork.Trip.Get(f => f.Id == id);
            if (trip == null)
            {
                return NotFound();
            }
            var model = new TripCreateVM
            {
                BusList = _unitOfWork.Bus.GetAll().Select(u => new SelectListItem
                {
                    Text = u.BusNumber,
                    Value = u.Id.ToString()
                }).ToList() // Приводим к списку, чтобы избежать проблем с ленивой загрузкой
            };
            // Найти рейс по ID
            

            return View(trip); // Отобразить форму редактирования
        }

        // POST: Trips/Edit/5
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Trip model)
        {
            if (!ModelState.IsValid)
            {
                return View(model); // Вернуть форму с ошибками
            }

            var tripToUpdate = _unitOfWork.Trip.Get(f => f.Id == id);
            if (tripToUpdate == null)
            {
                return NotFound();
            }

            // Обновить поля
            tripToUpdate.From = model.From;
            tripToUpdate.To = model.To;
            tripToUpdate.Depart = model.Depart;
            tripToUpdate.Return = model.Return;
            tripToUpdate.Time = model.Time;
            tripToUpdate.Guest = model.Guest;

            _unitOfWork.Trip.Update(tripToUpdate); // Сохранить изменения
            _unitOfWork.Save();

            return RedirectToAction("Index"); // Перенаправить на список рейсов
        }

        //// GET: Trips/Delete/5
        //public IActionResult Delete(int id)
        //{
        //    var Trip = _unitOfWork.Trip.Get(f => f.Id == id);
        //    if (Trip == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(Trip); // Отобразить подтверждение удаления
        //}

        // POST: Trips/Delete/5
        [HttpPost/*, ActionName("Delete")*/]
        //[ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var tripToDelete = _unitOfWork.Trip.Get(f => f.Id == id);
            if (tripToDelete == null)
            {
                return NotFound();
            }

            _unitOfWork.Trip.Remove(tripToDelete); // Удалить рейс
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index)); // Перенаправить на список рейсов
        }

        // GET: Trips/Login
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View(); // Отобразить форму входа
        }

       

        // POST: Trips/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).Wait();
            return RedirectToAction("Login"); // Перенаправить на форму входа
        }
    }


}
