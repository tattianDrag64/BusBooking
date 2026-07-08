using Azure.Core;
using Azure;
using BusBooking.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BusBooking.Repository.IRepositories;
using BusBooking.Repository;
using BusBooking.Models;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Core.Types;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Rendering;
using Mono.TextTemplating;

namespace BusBooking.Controllers
{
    public class TripController(IUnitOfWork unitOfWork, ILogger<TripRepository> logger) : Controller
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ILogger<TripRepository> _logger = logger;

        public IActionResult Index()
        {
            return Ok();
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
                    ModelState.AddModelError("", "No matching trip found.");
                    return BadRequest(ModelState);
                }
            }
            return Ok(model);
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
            return Ok(model);
        }

        [HttpPost]
        //[Authorize(Roles = "admin")]

        public IActionResult Create(TripCreateVM model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Получаем идентификатор пользователя

            if (ModelState.IsValid)
            {
                var bus = _unitOfWork.Bus.Get(b => b.Id == model.BusId);
                if (bus == null)
                {
                    ModelState.AddModelError("", "Selected bus not found.");
                    return BadRequest(ModelState);
                }

                // Преобразуем данные из модели представления в сущность Trip
                var newTrip = new Trip
                {
                    From = model.From,
                    To = model.To,
                    BusId = model.BusId,
                    RouteId = bus.RouteId,
                    DepartureDate = model.DepartureDate,
                    ArrivalDate = model.ArrivalDate,
                    IsReturnTrip = model.IsReturnTrip,
                    Price = model.Price
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


            return BadRequest(ModelState);
        }


        public IActionResult SearchTrips(TripVM model)
        {

            if (User.Identity?.IsAuthenticated != true)
            {
                // TempData kullanarak bir sonraki request'e kadar veri saklayabilirsiniz.
                TempData["Error"] = "You must be logged in to view this page.";
                return RedirectToAction("Index");
            }
            if (string.IsNullOrWhiteSpace(model.To))
            {
                //ModelState.AddModelError("", "Departure location cannot be empty.");
                TempData["Error"] = "Departure location cannot be empty.";
                return RedirectToAction("Index");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var trips = _unitOfWork.Trip.GetAll().Where(f => f.From == model.From && f.To == model.To);

            // Фильтрация по One Way или Return
            if (model.IsReturnTrip)
            {
                trips = trips.Where(f => f.DepartureDate.Date == model.DepartureDate.Date &&
                                          f.ArrivalDate.Date == model.ArrivalDate.Date);
            }
            else
            {
                trips = trips.Where(f => f.DepartureDate.Date == model.DepartureDate.Date);
            }

            // Преобразование данных в представление
            var tripResults = trips.Select(f => new TripCreateVM
            {
                TripId = f.Id,
                From = f.From,
                To = f.To,
                DepartureDate = f.DepartureDate,
                ArrivalDate = f.ArrivalDate,
                IsReturnTrip = f.IsReturnTrip,
                Price = f.Price
            }).ToList();

            return Ok(tripResults);
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
                    // Burada diğer gerekli alanları da ekleyebilirsiniz
                });

            return Ok(trips);
        }


        public IActionResult Edit(Guid id)
        {
            var trip = _unitOfWork.Trip.Get(f => f.Id == id);
            if (trip == null)
            {
                return NotFound();
            }
            var model = new TripCreateVM
            {
                BusList = [.. _unitOfWork.Bus.GetAll().Select(u => new SelectListItem
                {
                    Text = u.BusNumber,
                    Value = u.Id.ToString()
                })] // Приводим к списку, чтобы избежать проблем с ленивой загрузкой
            };
            // Найти рейс по ID


            return Ok(trip);
        }

        // POST: Trips/Edit/5
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public IActionResult Edit(Guid id, Trip model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var tripToUpdate = _unitOfWork.Trip.Get(f => f.Id == id);
            if (tripToUpdate == null)
            {
                return NotFound();
            }

            // Обновить поля
            tripToUpdate.From = model.From;
            tripToUpdate.To = model.To;
            tripToUpdate.DepartureDate = model.DepartureDate;
            tripToUpdate.ArrivalDate = model.ArrivalDate;
            tripToUpdate.IsReturnTrip = model.IsReturnTrip;
            tripToUpdate.Price = model.Price;

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
        public IActionResult Delete(Guid id)
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
            return Ok();
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
