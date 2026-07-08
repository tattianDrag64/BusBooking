namespace BusBookingDemo.Entity
{
    public class OrderSeat : BaseEntity
    {
        public required Guid OrderId { get; set; }
        public required Guid SeatDetailId { get; set; }
        public required Guid TripId { get; set; }

        public virtual Order Order { get; set; } = null!;
        public virtual SeatDetail SeatDetail { get; set; } = null!;
        public virtual Trip Trip { get; set; } = null!;

    }
}
