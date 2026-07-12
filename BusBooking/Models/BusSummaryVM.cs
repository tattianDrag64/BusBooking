namespace BusBooking.Models
{
    public class BusSummaryVM
    {
        public Guid Id { get; set; }
        public string BusNumber { get; set; } = string.Empty;
        public int SeatsCount { get; set; }
        public Guid RouteId { get; set; }
    }
}
