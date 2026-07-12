using System.ComponentModel.DataAnnotations;

namespace BusBooking.Models
{
    public class AdminCancelOrderVM
    {
        [Required]
        public required string Reason { get; set; }
    }
}
