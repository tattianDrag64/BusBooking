using BusBooking.Entity;
using BusBooking.Models;
using BusBooking.Repository.UnitOfWork;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusBooking.Controllers
{
    [ApiController]
    [Route("api/routes")]
    public class RoutesController(IUnitOfWork unitOfWork) : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        // GET api/routes
        [HttpGet]
        public IActionResult GetRoutes()
        {
            var routes = _unitOfWork.Route.GetAll(includeProperties: "Schedules").Select(ToSummaryVM);

            return Ok(routes);
        }

        // POST api/routes
        [HttpPost]
        [Authorize(Roles = nameof(UserRole.Admin), AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult Create([FromBody] RouteCreateVM model)
        {
            var route = new RouteInfo
            {
                DepartureCity = model.DepartureCity,
                ArrivalCity = model.ArrivalCity,
                Price = model.Price,
            };

            _unitOfWork.Route.Add(route);
            _unitOfWork.Save();

            return CreatedAtAction(nameof(GetRoutes), new { id = route.Id }, ToSummaryVM(route));
        }

        // PUT api/routes/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = nameof(UserRole.Admin), AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult Update(Guid id, [FromBody] RouteCreateVM model)
        {
            var route = _unitOfWork.Route.GetById(id);
            if (route == null)
            {
                return NotFound();
            }

            route.DepartureCity = model.DepartureCity;
            route.ArrivalCity = model.ArrivalCity;
            route.Price = model.Price;

            _unitOfWork.Route.Update(route);
            _unitOfWork.Save();

            return NoContent();
        }

        // DELETE api/routes/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = nameof(UserRole.Admin), AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult Delete(Guid id)
        {
            var route = _unitOfWork.Route.GetById(id);
            if (route == null)
            {
                return NotFound();
            }

            _unitOfWork.Route.Remove(route);
            _unitOfWork.Save();

            return NoContent();
        }

        // Schedule.Route <-> RouteInfo.Schedules EF fixup causes a JSON serializer cycle
        // if the raw entity is returned directly — same reason SchedulesController maps
        // to a flat VM instead of returning the entity as-is.
        private static RouteSummaryVM ToSummaryVM(RouteInfo route) => new()
        {
            Id = route.Id,
            DepartureCity = route.DepartureCity,
            ArrivalCity = route.ArrivalCity,
            Price = route.Price,
            Schedules = route.Schedules?.Select(s => new ScheduleSummaryVM
            {
                DayOfWeek = s.DayOfWeek,
                DepartureTime = s.DepartureTime.ToString(@"hh\:mm"),
                IsReturnTrip = s.IsReturnTrip,
            }) ?? [],
        };
    }
}
