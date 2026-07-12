namespace BusBooking.Models
{
    public class BookSeatRequestVM
    {
        public Guid TripId { get; set; }
        public List<Guid> SeatIds { get; set; } = [];
    }
}
