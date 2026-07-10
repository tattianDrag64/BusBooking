using BusBooking.Data;
using BusBooking.Entity;
using BusBooking.Repository.IRepositories;

namespace BusBooking.Repository
{
    public class RouteRepository(ApplicationDbContext context) : Repository<RouteInfo>(context), IRouteRepository
    {

        public IEnumerable<RouteInfo> GetAvaiableRoutes(string departureCity, string arrivalCity, DayOfWeek departureDay, DayOfWeek? returnDay = null)
        {
            return [.. Items.Where(r =>
                r.DepartureCity.ToLower() == departureCity.ToLower() &&
                r.ArrivalCity.ToLower() == arrivalCity.ToLower() &&
                r.DepartureDay == departureDay &&
                (returnDay == null || r.ReturnDay == returnDay))];
        }

        public bool RouteExists(string departureCity, string arrivalCity, DayOfWeek departureDay, TimeSpan departureTime)
        {
            return Items.Any(r =>
                r.DepartureCity.ToLower() == departureCity.ToLower() &&
                r.ArrivalCity.ToLower() == arrivalCity.ToLower() &&
                r.DepartureDay == departureDay &&
                r.DepartureTime == departureTime);
        }
    }
}