using BusBookingDemo.Entity;

namespace BusBookingDemo.Models
{
    public class OrderVM
    {
        public IEnumerable<Order> Orders { get; set; } = [];
    }
}
