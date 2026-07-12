namespace BusBooking.Entity
{
    public class RouteInfo : BaseEntity
    {
        public required string DepartureCity { get; set; }
        public required string ArrivalCity { get; set; }
        public decimal Price { get; set; }

        public virtual ICollection<Schedule> Schedules { get; set; } = [];
    }
}
