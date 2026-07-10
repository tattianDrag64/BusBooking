using BusBooking.Data;
using BusBooking.Entity;
using BusBooking.Repository.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Repository
{
    public class TripRepository(ApplicationDbContext context) : Repository<Trip>(context), ITripRepository
    {
        public IQueryable<Trip> Trips => Context.Set<Trip>();

        public IEnumerable<Trip> GetTripsByRoute(Guid routeId)
        {
            return Items.Where(t => t.RouteId == routeId).ToList();
        }

        public IEnumerable<Trip> GetTripsByDate(DateTime departureDate)
        {
            return Items.Where(t => t.DepartureDate.Date == departureDate.Date).ToList();
        }

        public bool TripExists(Guid routeId, DateTime departureDate, bool isReturnTrip)
        {
            return Items.Any(t =>
                t.RouteId == routeId &&
                t.DepartureDate.Date == departureDate.Date &&
                t.IsReturnTrip == isReturnTrip);
        }
    }
}
