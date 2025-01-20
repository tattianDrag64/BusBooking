using BusBooking.Entities;

namespace BusBooking.Repositories.IRepositories
{
    public interface ITripRepository : IRepository<Trip>
    {
        IEnumerable<Trip> GetTripsByRoute(int routeId);
        IEnumerable<Trip> GetTripsByDate(DateTime departureDate);
        bool TripExists(int routeId, DateTime departureDate, bool isReturnTrip);
        void Update(Trip obj);
    }
}
