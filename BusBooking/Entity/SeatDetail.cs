using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusBooking.Entity
{
    public class SeatDetail : BaseEntity
    {
        [Required]
        public bool IsOccupied { get; set; }
        public bool IsReserved { get; set; } = false;
        public DateTime? ReservedUntil { get; set; }
        public string? SeatNumber { get; set; }

        public required Guid TripId { get; set; }
        [ForeignKey("TripId")]
        [ValidateNever]
        public virtual Trip Trip { get; set; } = null!;
    }
}
