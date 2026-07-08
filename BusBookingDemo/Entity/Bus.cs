using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace BusBookingDemo.Entity
{
    public class Bus : BaseEntity
    {
        [Required]
        [DisplayName("Bus Number")]
        public required string BusNumber { get; set; }
        [DisplayName("Seats Count")]
        [Range(1, 80)]
        public int SeatsCount { get; set; }
        public required Guid RouteId { get; set; }
        public virtual RouteInfo Route { get; set; } = null!;
    }
}
