namespace BusBooking.Models
{
    public class SeatDetailVM
    {
        public Guid Id { get; set; }
        public string? SeatNumber { get; set; } // Artık string türünde
        public bool IsOccupied { get; set; }
    }
}
