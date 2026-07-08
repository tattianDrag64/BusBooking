using BusBooking.Entity;

namespace BusBooking.Models
{
    public class OrderVM
    {
        public IEnumerable<Order> Orders { get; set; } = [];
    }
}
