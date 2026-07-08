namespace BusBookingDemo.Entity
{
    public class Order : BaseEntity
    {
        public required string OrderCode { get; set; }
        public required Guid UserId { get; set; }
        public required Guid RouteId { get; set; }
        public required Guid TripId { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual User User { get; set; } = null!;
        public virtual RouteInfo Route { get; set; } = null!;
        public virtual Trip Trip { get; set; } = null!;
        public virtual ICollection<OrderSeat> OrderSeats { get; set; } = [];
    }
}
