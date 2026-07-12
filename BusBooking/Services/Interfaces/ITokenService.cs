using BusBooking.Entity;

namespace BusBooking.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();

        // SHA-256 of the raw refresh token — what's actually stored in the DB, so a
        // database compromise alone doesn't hand over usable session tokens.
        string HashRefreshToken(string rawToken);
    }
}