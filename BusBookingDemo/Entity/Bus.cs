using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace BusBookingDemo.Entity
{
    public class Bus 
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [DisplayName("Bus Number")]
        public string BusNumber { get; set; }
        [DisplayName("Seats Count")]
        [Range(1, 80)]
        public int SeatsCount { get; set; }
    }
}
