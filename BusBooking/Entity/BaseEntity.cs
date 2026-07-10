using System.ComponentModel.DataAnnotations;

namespace BusBooking.Entity
{
    public abstract class BaseEntity
    {
        [Key]
        public Guid Id { get; set; } = Guid.CreateVersion7();

    }
}
