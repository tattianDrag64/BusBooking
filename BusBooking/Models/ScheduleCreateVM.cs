using System.ComponentModel.DataAnnotations;

namespace BusBooking.Models
{
    public class ScheduleCreateVM
    {
        [Required]
        public Guid RouteId { get; set; }

        [Required]
        public TimeSpan DepartureTime { get; set; }

        [Required]
        public TimeSpan Duration { get; set; }

        // null = runs every day.
        public DayOfWeek? DayOfWeek { get; set; }

        public bool IsReturnTrip { get; set; }
    }
}
