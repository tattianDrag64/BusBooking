using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusBookingDemo.Entity
{
    public class User 
    {
        [Key]
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        public string Phone { get; set; }
        [NotMapped]
        public string Role { get; set; }
    }
}
