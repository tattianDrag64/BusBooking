using BusBookingDemo.Data;
using BusBookingDemo.Entity;
using BusBookingDemo.Repository.IRepositories;

namespace BusBookingDemo.Repository
{
    public class RouteRepository(ApplicationDbContext context) : Repository<RouteInfo>(context), IRouteRepository
    {

        public IEnumerable<RouteInfo> GetAvaiableRoutes(string departureCity, string arrivalCity, DayOfWeek departureDay, DayOfWeek? returnDay = null)
        {
            return [.. Items.Where(r =>
                r.DepartureCity == departureCity &&
                r.ArrivalCity == arrivalCity &&
                r.DepartureDay == departureDay &&
                (returnDay == null || r.ReturnDay == returnDay))];
        }

        public bool RouteExists(string departureCity, string arrivalCity, DayOfWeek departureDay, TimeSpan departureTime)
        {
            return Items.Any(r =>
                r.DepartureCity == departureCity &&
                r.ArrivalCity == arrivalCity &&
                r.DepartureDay == departureDay &&
                r.DepartureTime == departureTime);
        }

        public void Update(RouteInfo obj)
        {
            Items.Update(obj);
        }
    }
}
