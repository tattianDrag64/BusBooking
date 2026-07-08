using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace BusBooking.Models
{
    public class TripCreateVM
    {
        [Key]
        public Guid TripId { get; set; }
        [Required]
        [Display(Name = "From")]
        public string From { get; set; } = string.Empty;
        [Required]
        [Display(Name = "To")]
        public string To { get; set; } = string.Empty;
        [Required]
        [Display(Name = "Departure Date")]
        public DateTime DepartureDate { get; set; }

        [Display(Name = "Arrival Date")]
        public DateTime ArrivalDate { get; set; }

        public bool IsReturnTrip { get; set; }
        public double Price { get; set; }
        [ValidateNever]
        [Display(Name = "Bus")]
        public Guid BusId { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> BusList { get; set; } = [];
    }
}
