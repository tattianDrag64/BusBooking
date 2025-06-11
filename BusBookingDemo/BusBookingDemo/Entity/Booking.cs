using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusBookingDemo.Entity
{
    public class Booking 
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required]
        public int TripId { get; set; }
        [Required]
        public int SeatDetailId { get; set; }

        [ForeignKey("UserId")]
        [ValidateNever]
        public User User { get; set; }
        [ForeignKey("TripId")]
        [ValidateNever]
        public Trip Trip { get; set; }
        [ForeignKey("SeatDetailId")]
        [ValidateNever]
        public SeatDetail SeatDetail { get; set; }
    }
}
