using System.ComponentModel.DataAnnotations;

namespace BusBooking.Models
{
    public class RouteCreateVM
    {
        [Required]
        public required string DepartureCity { get; set; }

        [Required]
        public required string ArrivalCity { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }
    }
}
