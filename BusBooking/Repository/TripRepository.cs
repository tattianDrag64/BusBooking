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

        // Exact timestamp match, not just the date — a route can now have multiple
        // Schedule slots on the same day (e.g. 10:00, 13:00, 16:00), so comparing by
        // date alone would treat those as duplicates of each other.
        public bool TripExists(Guid routeId, DateTime departureDate, bool isReturnTrip)
        {
            return Items.Any(t =>
                t.RouteId == routeId &&
                t.DepartureDate == departureDate &&
                t.IsReturnTrip == isReturnTrip);
        }
    }
}
