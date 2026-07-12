using System.ComponentModel.DataAnnotations;

namespace BusBooking.Models
{
    public class BusCreateVM
    {
        [Required]
        public required string BusNumber { get; set; }

        [Range(1, 80)]
        public int SeatsCount { get; set; }

        [Required]
        public Guid RouteId { get; set; }
    }
}
