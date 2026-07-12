namespace BusBooking.Models
{
    public class ScheduleVM
    {
        public Guid Id { get; set; }
        public Guid RouteId { get; set; }
        public TimeSpan DepartureTime { get; set; }
        public TimeSpan Duration { get; set; }
        public DayOfWeek? DayOfWeek { get; set; }
        public bool IsReturnTrip { get; set; }
    }
}
