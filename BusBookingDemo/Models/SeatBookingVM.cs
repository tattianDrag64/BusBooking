namespace BusBookingDemo.Models
{
    public class SeatBookingVM
    {
        public Guid TripId { get; set; }
        public List<SeatDetailVM> Seats { get; set; } = [];
        public Guid SelectedSeat { get; set; }
    }
}