namespace BusBooking.Models
{
    public class ScheduleSummaryVM
    {
        // null = runs every day.
        public DayOfWeek? DayOfWeek { get; set; }
        public string DepartureTime { get; set; } = string.Empty;
        public bool IsReturnTrip { get; set; }
    }
}
