using BusBooking.Data;
using BusBooking.Entity;
using BusBooking.Repository.IRepositories;

namespace BusBooking.Repository
{
    public class ScheduleRepository(ApplicationDbContext context) : Repository<Schedule>(context), IScheduleRepository
    {
        public IEnumerable<Schedule> GetByRoute(Guid routeId)
        {
            return [.. Items.Where(s => s.RouteId == routeId)];
        }

        public bool ScheduleExists(Guid routeId, DayOfWeek? dayOfWeek, TimeSpan departureTime, bool isReturnTrip)
        {
            return Items.Any(s =>
                s.RouteId == routeId &&
                s.DayOfWeek == dayOfWeek &&
                s.DepartureTime == departureTime &&
                s.IsReturnTrip == isReturnTrip);
        }
    }
}
