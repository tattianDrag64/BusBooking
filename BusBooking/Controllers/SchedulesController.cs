using BusBooking.Entity;
using BusBooking.Models;
using BusBooking.Repository.UnitOfWork;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusBooking.Controllers
{
    [ApiController]
    [Route("api/schedules")]
    [Authorize(Roles = nameof(UserRole.Admin), AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class SchedulesController(IUnitOfWork unitOfWork) : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        // POST api/schedules
        [HttpPost]
        public IActionResult Create([FromBody] ScheduleCreateVM model)
        {
            var route = _unitOfWork.Route.GetById(model.RouteId);
            if (route == null)
            {
                return NotFound(new { message = "Route not found." });
            }

            if (_unitOfWork.Schedule.ScheduleExists(model.RouteId, model.DayOfWeek, model.DepartureTime, model.IsReturnTrip))
            {
                return Conflict(new { message = "A schedule slot with the same route, day and departure time already exists." });
            }

            var schedule = new Schedule
            {
                RouteId = model.RouteId,
                DepartureTime = model.DepartureTime,
                Duration = model.Duration,
                DayOfWeek = model.DayOfWeek,
                IsReturnTrip = model.IsReturnTrip,
            };
            _unitOfWork.Schedule.Add(schedule);
            _unitOfWork.Save();

            return CreatedAtAction(nameof(GetByRoute), new { routeId = model.RouteId }, ToVM(schedule));
        }

        // GET api/schedules/route/{routeId}
        [HttpGet("route/{routeId}")]
        public IActionResult GetByRoute(Guid routeId)
        {
            return Ok(_unitOfWork.Schedule.GetByRoute(routeId).Select(ToVM));
        }

        // EF fixes up Schedule.Route <-> RouteInfo.Schedules automatically once both are
        // tracked in the same context — returning the raw entity here throws a JSON
        // serializer cycle exception, hence mapping to a flat VM instead.
        private static ScheduleVM ToVM(Schedule schedule) => new()
        {
            Id = schedule.Id,
            RouteId = schedule.RouteId,
            DepartureTime = schedule.DepartureTime,
            Duration = schedule.Duration,
            DayOfWeek = schedule.DayOfWeek,
            IsReturnTrip = schedule.IsReturnTrip,
        };

        // DELETE api/schedules/{id}
        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            var schedule = _unitOfWork.Schedule.GetById(id);
            if (schedule == null)
            {
                return NotFound();
            }

            _unitOfWork.Schedule.Remove(schedule);
            _unitOfWork.Save();

            return NoContent();
        }
    }
}
