namespace BusBookingDemo.Models
{
    public class SeatBookingVM
    {
        public int TripId { get; set; }
        public List<SeatDetailVM> Seats { get; set; }
        public int SelectedSeat { get; set; } //
    }
}
