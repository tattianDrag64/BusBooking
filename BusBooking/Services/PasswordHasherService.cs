using BusBooking.Services.Interfaces;
using static BCrypt.Net.BCrypt;

namespace BusBooking.Services
{
    public class PasswordHasherService : IPasswordHasherService
    {
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        }
        public bool VerifyPassword(string inputPassword, string storedHash)
        {
            return Verify(inputPassword, storedHash);
        }
    }
}