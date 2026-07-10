using BusBooking.Entity;
using BusBooking.Models;
using BusBooking.Repository.UnitOfWork;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusBooking.Controllers
{
    [ApiController]
    [Route("api/trips")]
    public class TripsController(IUnitOfWork unitOfWork) : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        // GET api/trips/search?from=Sofia&to=Berlin&departureDate=2026-08-01&arrivalDate=2026-08-01&isReturnTrip=false
        [HttpGet("search")]
        public IActionResult SearchTrips([FromQuery] TripVM model)
        {
            var trips = _unitOfWork.Trip.GetAll().Where(f =>
                f.From.ToLower() == model.From.ToLower() && f.To.ToLower() == model.To.ToLower());

            trips = model.IsReturnTrip
                ? trips.Where(f => f.DepartureDate.Date == model.DepartureDate.Date && f.ArrivalDate.Date == model.ArrivalDate.Date)
                : trips.Where(f => f.DepartureDate.Date == model.DepartureDate.Date);

            var results = trips.Select(f => new TripCreateVM
            {
                TripId = f.Id,
                From = f.From,
                To = f.To,
                DepartureDate = f.DepartureDate,
                ArrivalDate = f.ArrivalDate,
                IsReturnTrip = f.IsReturnTrip,
                Price = f.Price,
                BusId = f.BusId,
            }).ToList();

            return Ok(results);
        }

        // GET api/trips
        [HttpGet]
        [Authorize(Roles = nameof(UserRole.Admin), AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult ListTrips()
        {
            var trips = _unitOfWork.Trip.GetAll().Select(t => new TripCreateVM
            {
                TripId = t.Id,
                From = t.From,
                To = t.To,
                DepartureDate = t.DepartureDate,
                ArrivalDate = t.ArrivalDate,
                IsReturnTrip = t.IsReturnTrip,
                Price = t.Price,
                BusId = t.BusId,
            });

            return Ok(trips);
        }

        // POST api/trips
        [HttpPost]
        [Authorize(Roles = nameof(UserRole.Admin), AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult Create([FromBody] TripCreateVM model)
        {
            var bus = _unitOfWork.Bus.GetById(model.BusId);
            if (bus == null)
            {
                return BadRequest(new { message = "Selected bus not found." });
            }

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

            _unitOfWork.Trip.Add(newTrip);
            _unitOfWork.Save();

            return CreatedAtAction(nameof(ListTrips), new { id = newTrip.Id }, newTrip);
        }

        // PUT api/trips/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = nameof(UserRole.Admin), AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult Edit(Guid id, [FromBody] TripEditVM model)
        {
            var tripToUpdate = _unitOfWork.Trip.GetById(id);
            if (tripToUpdate == null)
            {
                return NotFound();
            }

            tripToUpdate.From = model.From;
            tripToUpdate.To = model.To;
            tripToUpdate.DepartureDate = model.DepartureDate;
            tripToUpdate.ArrivalDate = model.ArrivalDate;
            tripToUpdate.IsReturnTrip = model.IsReturnTrip;
            tripToUpdate.Price = model.Price;

            _unitOfWork.Trip.Update(tripToUpdate);
            _unitOfWork.Save();

            return NoContent();
        }

        // DELETE api/trips/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = nameof(UserRole.Admin), AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult Delete(Guid id)
        {
            var tripToDelete = _unitOfWork.Trip.GetById(id);
            if (tripToDelete == null)
            {
                return NotFound();
            }

            _unitOfWork.Trip.Remove(tripToDelete);
            _unitOfWork.Save();

            return NoContent();
        }
    }
}
