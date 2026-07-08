namespace BusBookingDemo.Entity
{
    public class RouteInfo : BaseEntity
    {
        public required string DepartureCity { get; set; }
        public required string ArrivalCity { get; set; }
        public DayOfWeek DepartureDay { get; set; }
        public DayOfWeek ReturnDay { get; set; }
        public TimeSpan DepartureTime { get; set; }
        public TimeSpan ReturnTime { get; set; }
        public decimal Price { get; set; }

    }
}
