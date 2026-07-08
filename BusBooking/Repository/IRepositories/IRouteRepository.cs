using BusBooking.Entity;

namespace BusBooking.Repository.IRepositories
{
    public interface IRouteRepository : IRepository<RouteInfo>
    {
        IEnumerable<RouteInfo> GetAvaiableRoutes(string departureCity, string arrivalCity, DayOfWeek departureDay, DayOfWeek? returnDay = null);
        bool RouteExists(string departureCity, string arrivalCity, DayOfWeek departureDay, TimeSpan departureTime);
        void Update(RouteInfo obj);
    }
}
