namespace BusBooking.Repositories.IRepositories
{
    public interface IRouteRepository : IRepository<Entities.Route>
    {
        IEnumerable<Route> GetAvaiableRoutes(string departureCity, string arrivalCity, DayOfWeek departureDay, DayOfWeek? returnDay = null);
        bool RouteExists(string departureCity, string arrivalCity, DayOfWeek departureDay,TimeSpan departureTime);
        void Update(Entities.Route obj);
    }
}
