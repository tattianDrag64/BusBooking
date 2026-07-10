using BusBooking.Entity;
using BusBooking.Models;
using BusBooking.Repository.UnitOfWork;
using BusBooking.Services.ServiceManager;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusBooking.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController(IUnitOfWork unitOfWork, IServiceManager services, IConfiguration configuration) : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IServiceManager _services = services;
        private readonly IConfiguration _configuration = configuration;

        [HttpPost("signup")]
        public IActionResult SignUp([FromBody] SignUpVM model)
        {
            var existing = _unitOfWork.User.Get(x =>
                x.Username.ToLower() == model.Username.ToLower() || x.Email.ToLower() == model.Email.ToLower());
            if (existing != null)
            {
                return Conflict(new { message = "Username or email already exists." });
            }

            var user = new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Username = model.Username,
                Phone = model.Phone,
                Email = model.Email,
                PasswordHash = _services.PasswordHasherService.HashPassword(model.Password),
                Role = UserRole.Customer,
            };
            _unitOfWork.User.Add(user);
            _unitOfWork.Save();

            return Ok(IssueTokens(user));
        }

        [HttpPost("signin")]
        public IActionResult SignIn([FromBody] SignInVM model)
        {
            var user = _unitOfWork.User.Get(x => x.Email.ToLower() == model.Email.ToLower());
            if (user == null || !_services.PasswordHasherService.VerifyPassword(model.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            return Ok(IssueTokens(user));
        }

        [HttpPost("refresh")]
        public IActionResult Refresh([FromBody] RefreshRequestVM model)
        {
            var refreshToken = _unitOfWork.RefreshToken.GetByToken(model.RefreshToken);
            if (refreshToken == null || refreshToken.IsRevoked || refreshToken.Expires < DateTime.UtcNow)
            {
                return Unauthorized(new { message = "Invalid or expired refresh token." });
            }

            var user = _unitOfWork.User.GetById(refreshToken.UserId);
            if (user == null)
            {
                return Unauthorized(new { message = "User not found." });
            }

            refreshToken.IsRevoked = true;
            _unitOfWork.RefreshToken.Update(refreshToken);
            _unitOfWork.Save();

            return Ok(IssueTokens(user));
        }

        [HttpPost("revoke")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult Revoke([FromBody] RefreshRequestVM model)
        {
            var refreshToken = _unitOfWork.RefreshToken.GetByToken(model.RefreshToken);
            if (refreshToken == null)
            {
                return NotFound();
            }

            refreshToken.IsRevoked = true;
            _unitOfWork.RefreshToken.Update(refreshToken);
            _unitOfWork.Save();

            return NoContent();
        }

        private TokenResponseVM IssueTokens(User user)
        {
            var accessToken = _services.TokenService.GenerateAccessToken(user);
            var refreshTokenValue = _services.TokenService.GenerateRefreshToken();

            _unitOfWork.RefreshToken.Add(new RefreshToken
            {
                UserId = user.Id,
                Token = refreshTokenValue,
                Expires = DateTime.UtcNow.AddDays(double.Parse(_configuration["Jwt:RefreshTokenDays"]!))
            });
            _unitOfWork.Save();

            return new TokenResponseVM
            {
                AccessToken = accessToken,
                RefreshToken = refreshTokenValue
            };
        }
    }
}
