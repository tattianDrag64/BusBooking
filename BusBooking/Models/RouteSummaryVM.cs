namespace BusBooking.Models
{
    public class RouteSummaryVM
    {
        public Guid Id { get; set; }
        public string DepartureCity { get; set; } = string.Empty;
        public string ArrivalCity { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public IEnumerable<ScheduleSummaryVM> Schedules { get; set; } = [];
    }
}
