using BusBooking.Entity;

namespace BusBooking.Models
{
    public class AdminOrderSummaryVM
    {
        public Guid Id { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string From { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
        public DateTime DepartureDate { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; }
        public IEnumerable<string?> SeatNumbers { get; set; } = [];
        public OrderStatus Status { get; set; }
    }
}
