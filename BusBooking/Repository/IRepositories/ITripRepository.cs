using BusBooking.Entity;

namespace BusBooking.Repository.IRepositories
{
    public interface ITripRepository : IRepository<Trip>
    {
        IQueryable<Trip> Trips { get; }
        IEnumerable<Trip> GetTripsByRoute(Guid routeId);
        IEnumerable<Trip> GetTripsByDate(DateTime departureDate);
        bool TripExists(Guid routeId, DateTime departureDate, bool isReturnTrip);
    }
}