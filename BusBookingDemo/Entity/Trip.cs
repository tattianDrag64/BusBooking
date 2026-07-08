using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusBookingDemo.Entity
{
    public class Trip : BaseEntity
    {
        [Required]
        public required string From { get; set; }
        [Required]
        public required string To { get; set; }
        public required Guid BusId { get; set; }
        [ForeignKey("BusId")]
        [ValidateNever]
        public virtual Bus Bus { get; set; } = null!;

        public int ScheduleId { get; set; }
        public DateTime DepartureDate { get; set; }
        public DateTime ArrivalDate { get; set; }
        public bool IsReturnTrip { get; set; }

        public required Guid RouteId { get; set; }
        [ForeignKey("RouteId")]
        [ValidateNever]
        public virtual RouteInfo Route { get; set; } = null!;

        public double Price { get; set; }
    }
}
