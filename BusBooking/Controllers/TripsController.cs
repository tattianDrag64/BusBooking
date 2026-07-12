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

        // GET api/trips?page=1&pageSize=20
        [HttpGet]
        [Authorize(Roles = nameof(UserRole.Admin), AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult ListTrips([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var totalCount = _unitOfWork.Trip.Count();
            var trips = _unitOfWork.Trip
                .GetPage((page - 1) * pageSize, pageSize)
                .Select(t => new TripCreateVM
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

            return Ok(new { items = trips, page, pageSize, totalCount });
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
                DepartureDate = DateTime.SpecifyKind(model.DepartureDate, DateTimeKind.Utc),
                ArrivalDate = DateTime.SpecifyKind(model.ArrivalDate, DateTimeKind.Utc),
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
            tripToUpdate.DepartureDate = DateTime.SpecifyKind(model.DepartureDate, DateTimeKind.Utc);
            tripToUpdate.ArrivalDate = DateTime.SpecifyKind(model.ArrivalDate, DateTimeKind.Utc);
            tripToUpdate.IsReturnTrip = model.IsReturnTrip;
            tripToUpdate.Price = model.Price;

            _unitOfWork.Trip.Update(tripToUpdate);
            _unitOfWork.Save();

            return NoContent();
        }

        // POST api/trips/generate — creates Trip rows for the next N weeks from every
        // Schedule slot across all routes, skipping ones that already exist.
        [HttpPost("generate")]
        [Authorize(Roles = nameof(UserRole.Admin), AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GenerateTrips([FromBody] TripGenerateRequestVM model)
        {
            var weeksAhead = model.WeeksAhead > 0 ? model.WeeksAhead : 4;
            var today = DateTime.UtcNow.Date;
            var endDate = today.AddDays(weeksAhead * 7);

            var schedules = _unitOfWork.Schedule.GetAll(includeProperties: "Route");

            var created = 0;
            var skipped = 0;
            var skippedNoBus = 0;

            foreach (var schedule in schedules)
            {
                var bus = _unitOfWork.Bus.GetAll(b => b.RouteId == schedule.RouteId).FirstOrDefault();
                if (bus == null)
                {
                    // No bus assigned to this route yet — nothing to generate trips for.
                    skippedNoBus++;
                    continue;
                }

                for (var date = today; date < endDate; date = date.AddDays(1))
                {
                    if (schedule.DayOfWeek != null && date.DayOfWeek != schedule.DayOfWeek)
                    {
                        continue;
                    }

                    var departureDate = DateTime.SpecifyKind(date.Add(schedule.DepartureTime), DateTimeKind.Utc);

                    if (_unitOfWork.Trip.TripExists(schedule.RouteId, departureDate, schedule.IsReturnTrip))
                    {
                        skipped++;
                        continue;
                    }

                    var (from, to) = schedule.IsReturnTrip
                        ? (schedule.Route.ArrivalCity, schedule.Route.DepartureCity)
                        : (schedule.Route.DepartureCity, schedule.Route.ArrivalCity);

                    _unitOfWork.Trip.Add(new Trip
                    {
                        From = from,
                        To = to,
                        BusId = bus.Id,
                        RouteId = schedule.RouteId,
                        DepartureDate = departureDate,
                        ArrivalDate = departureDate.Add(schedule.Duration),
                        IsReturnTrip = schedule.IsReturnTrip,
                        Price = (double)schedule.Route.Price,
                    });
                    created++;
                }
            }

            _unitOfWork.Save();

            return Ok(new { created, skipped, skippedNoBus });
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
