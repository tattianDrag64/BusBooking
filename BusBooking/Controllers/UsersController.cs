using BusBooking.Entity;
using BusBooking.Models;
using BusBooking.Repository.UnitOfWork;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusBooking.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize(Roles = nameof(UserRole.Admin), AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UsersController(IUnitOfWork unitOfWork) : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        // GET api/users
        [HttpGet]
        public IActionResult GetUsers()
        {
            var users = _unitOfWork.User.GetAll().Select(u => new UserSummaryVM
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Username = u.Username,
                Email = u.Email,
                Phone = u.Phone,
                Role = u.Role,
            });

            return Ok(users);
        }
    }
}
