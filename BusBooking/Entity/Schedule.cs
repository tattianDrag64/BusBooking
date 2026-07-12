namespace BusBooking.Entity
{
    public class Schedule : BaseEntity
    {
        public required Guid RouteId { get; set; }
        public virtual RouteInfo Route { get; set; } = null!;

        public TimeSpan DepartureTime { get; set; }

        // Used to compute Trip.ArrivalDate = DepartureDate + Duration when auto-generating trips.
        public TimeSpan Duration { get; set; }

        // null = runs every day; a specific value = runs only on that day of the week.
        public DayOfWeek? DayOfWeek { get; set; }

        // false = RouteInfo.DepartureCity -> ArrivalCity, true = the reverse leg.
        public bool IsReturnTrip { get; set; }
    }
}
