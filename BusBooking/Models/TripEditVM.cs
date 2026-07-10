using System.ComponentModel.DataAnnotations;

namespace BusBooking.Models
{
    public class TripEditVM
    {
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
    }
}
