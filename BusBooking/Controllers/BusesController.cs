using BusBooking.Entity;
using BusBooking.Models;
using BusBooking.Repository.UnitOfWork;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusBooking.Controllers
{
    [ApiController]
    [Route("api/buses")]
    [Authorize(Roles = nameof(UserRole.Admin), AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class BusesController(IUnitOfWork unitOfWork) : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        // GET api/buses
        [HttpGet]
        public IActionResult GetBuses()
        {
            var buses = _unitOfWork.Bus.GetAll().Select(ToVM);

            return Ok(buses);
        }

        // POST api/buses
        [HttpPost]
        public IActionResult Create([FromBody] BusCreateVM model)
        {
            var route = _unitOfWork.Route.GetById(model.RouteId);
            if (route == null)
            {
                return BadRequest(new { message = "Route not found." });
            }

            var bus = new Bus
            {
                BusNumber = model.BusNumber,
                SeatsCount = model.SeatsCount,
                RouteId = model.RouteId,
            };

            _unitOfWork.Bus.Add(bus);
            _unitOfWork.Save();

            return CreatedAtAction(nameof(GetBuses), new { id = bus.Id }, ToVM(bus));
        }

        // DELETE api/buses/{id}
        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            var bus = _unitOfWork.Bus.GetById(id);
            if (bus == null)
            {
                return NotFound();
            }

            _unitOfWork.Bus.Remove(bus);
            _unitOfWork.Save();

            return NoContent();
        }

        private static BusSummaryVM ToVM(Bus bus) => new()
        {
            Id = bus.Id,
            BusNumber = bus.BusNumber,
            SeatsCount = bus.SeatsCount,
            RouteId = bus.RouteId,
        };
    }
}
