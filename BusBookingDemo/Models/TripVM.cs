using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace BusBookingDemo.Models
{
    public class TripVM
    {
        public bool IsReturnTrip { get; set; }
        [Required]
        [Display(Name = "From")]
        public required string From { get; set; }
        [Required]
        [Display(Name = "To")]
        public required string To { get; set; }
        [Required]
        [Display(Name = "Departure Date")]
        public DateTime DepartureDate { get; set; }
        [Required]
        [Display(Name = "Arrival Date")]
        public DateTime ArrivalDate { get; set; }
        public double Price { get; set; }
    }
}
