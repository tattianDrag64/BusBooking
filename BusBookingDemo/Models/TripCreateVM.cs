using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace BusBookingDemo.Models
{
    public class TripCreateVM
    {
        [Key]
        public int TripId { get; set; }
        [Required]
        [Display(Name = "From")]
        public string? From { get; set; }
        [Required]
        [Display(Name = "To")]
        public string? To { get; set; }
        [Required]
        [Display(Name = "Depart")]
        public string? Depart { get; set; }

        [Display(Name = "Return")]
        public string? Return { get; set; }

        [Display(Name = "Time")]
        public string? Time { get; set; }
        [Required]
        [Display(Name = "Guest")]
        public string? Guest { get; set; }
        [ValidateNever]
        [Display(Name = "Bus")]
        public int BusId { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> BusList { get; set; }
    }
}
