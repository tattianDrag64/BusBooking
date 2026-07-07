namespace BusBookingDemo.Models
{
    public class SeatDetailVM
    {
        public int Id { get; set; }
        public string SeatNumber { get; set; } // Artık string türünde
        public bool IsOccupied { get; set; }
    }
}
