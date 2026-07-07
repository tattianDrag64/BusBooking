using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace BusBookingDemo.Entity
{
    public class Bus : BaseEntity
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [DisplayName("Bus Number")]
        public string BusNumber { get; set; }
        [DisplayName("Seats Count")]
        [Range(1, 80)]
        public int SeatsCount { get; set; }
        public int RouteId { get; set; }

        public virtual Route Route { get; set; }
    }
}
