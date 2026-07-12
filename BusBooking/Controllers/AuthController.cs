using BusBooking.Entity;
using BusBooking.Models;
using BusBooking.Repository.UnitOfWork;
using BusBooking.Services.ServiceManager;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BusBooking.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController(
        IUnitOfWork unitOfWork,
        IServiceManager services,
        IConfiguration configuration,
        ILogger<AuthController> logger) : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IServiceManager _services = services;
        private readonly IConfiguration _configuration = configuration;
        private readonly ILogger<AuthController> _logger = logger;

        private const int MaxFailedLoginAttempts = 5;
        private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

        [HttpPost("signup")]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> SignUp([FromBody] SignUpVM model)
        {
            var existing = _unitOfWork.User.Get(x =>
                x.Username.ToLower() == model.Username.ToLower() || x.Email.ToLower() == model.Email.ToLower());
            if (existing != null)
            {
                return Conflict(new { message = "Username or email already exists." });
            }

            if (await _services.PasswordBreachChecker.IsPwnedAsync(model.Password, HttpContext.RequestAborted))
            {
                return BadRequest(new { message = "This password has appeared in a known data breach. Please choose a different one." });
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
        [EnableRateLimiting("auth")]
        public IActionResult SignIn([FromBody] SignInVM model)
        {
            var user = _unitOfWork.User.Get(x => x.Email.ToLower() == model.Email.ToLower(), tracked: true);
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            if (user.LockoutEnd != null && user.LockoutEnd > DateTime.UtcNow)
            {
                return StatusCode(StatusCodes.Status423Locked, new
                {
                    message = "Account temporarily locked due to too many failed sign-in attempts. Try again later.",
                    lockoutEnd = user.LockoutEnd,
                });
            }

            if (!_services.PasswordHasherService.VerifyPassword(model.Password, user.PasswordHash))
            {
                user.FailedLoginAttempts++;
                if (user.FailedLoginAttempts >= MaxFailedLoginAttempts)
                {
                    user.LockoutEnd = DateTime.UtcNow.Add(LockoutDuration);
                }
                _unitOfWork.User.Update(user);
                _unitOfWork.Save();

                return Unauthorized(new { message = "Invalid email or password." });
            }

            user.FailedLoginAttempts = 0;
            user.LockoutEnd = null;
            _unitOfWork.User.Update(user);
            _unitOfWork.Save();

            return Ok(IssueTokens(user));
        }

        [HttpPost("refresh")]
        public IActionResult Refresh([FromBody] RefreshRequestVM model)
        {
            var refreshToken = _unitOfWork.RefreshToken.GetByToken(_services.TokenService.HashRefreshToken(model.RefreshToken));
            if (refreshToken == null)
            {
                return Unauthorized(new { message = "Invalid or expired refresh token." });
            }

            if (refreshToken.IsRevoked)
            {
                // Reuse of an already-rotated/revoked token — the standard signal that a
                // refresh token was stolen. Kill every other active session for this user
                // too, not just reject this one request.
                _unitOfWork.RefreshToken.RevokeAllForUser(refreshToken.UserId);
                _logger.LogWarning(
                    "Refresh token reuse detected for user {UserId} from {RemoteIp} — all active sessions revoked.",
                    refreshToken.UserId,
                    HttpContext.Connection.RemoteIpAddress);
                return Unauthorized(new { message = "Invalid or expired refresh token." });
            }

            if (refreshToken.Expires < DateTime.UtcNow)
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
            var refreshToken = _unitOfWork.RefreshToken.GetByToken(_services.TokenService.HashRefreshToken(model.RefreshToken));
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
                Token = _services.TokenService.HashRefreshToken(refreshTokenValue),
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
