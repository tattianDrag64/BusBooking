using System.ComponentModel.DataAnnotations;

namespace BusBookingDemo.Models
{
    public class SignUpVM
    {
        [Required]
        [Display(Name = "First Name")]
        public required string FirstName { get; set; }
        [Required]
        [Display(Name = "Last Name")]
        public required string LastName { get; set; }
        [Required]
        [Display(Name = "Username")]
        public required string Username { get; set; }

        [Required]
        [Display(Name = "Phone")]
        public required string Phone { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "E-mail")]
        public required string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public required string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Your password does not match.")]
        [Display(Name = "Password Repeat ")]
        public required string ConfirmPassword { get; set; }
    }
}
