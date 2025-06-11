using BusBookingDemo.Entity;

namespace BusBookingDemo.Models
{
    public class BookingVM
    {
        public IEnumerable<Booking> Bookings { get; set; }
    }
}
