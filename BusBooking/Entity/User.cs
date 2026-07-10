using System.ComponentModel.DataAnnotations;

namespace BusBooking.Entity
{
    public class User : BaseEntity
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Username { get; set; }
        public required string PasswordHash { get; set; }
        [EmailAddress]
        public required string Email { get; set; }
        public string? Phone { get; set; }
        public required UserRole Role { get; set; }
    }
}
