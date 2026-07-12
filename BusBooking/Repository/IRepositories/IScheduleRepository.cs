using BusBooking.Entity;

namespace BusBooking.Repository.IRepositories
{
    public interface IScheduleRepository : IRepository<Schedule>
    {
        IEnumerable<Schedule> GetByRoute(Guid routeId);
        bool ScheduleExists(Guid routeId, DayOfWeek? dayOfWeek, TimeSpan departureTime, bool isReturnTrip);
    }
}
