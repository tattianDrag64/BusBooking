using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusBookingDemo.Entity
{
    public class SeatDetail 
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public bool IsOccupied { get; set; } 

        public string? SeatNumber { get; set; }

        [ForeignKey("TripId")]
        [ValidateNever]
        public int TripId { get; set; } 
        public Trip Trip { get; set; }


        //[ForeignKey("BusId")]
        //[ValidateNever]
        //public int BusId { get; set; }
        //public Bus Bus { get; set; }

    }
}
