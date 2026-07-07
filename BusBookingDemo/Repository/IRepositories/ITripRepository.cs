using BusBookingDemo.Entity;

namespace BusBookingDemo.Repository.IRepositories
{
    public interface ITripRepository : IRepository<Trip>
    {
        IQueryable<Trip> Trips { get; }
        //IEnumerable<Trip> GetTripsByDirection(int directionId);
        //IEnumerable<Trip> GetTripsByDate(DateTime departureDate);
        ////bool TripExists(int routeId, DateTime departureDate, bool isReturnTrip);
        //public void CompleteTrip(int tripId);

        //void getTrip(Trip Trip);
        //void editTrip(Trip Trip);

        //Task DeleteTrip(Trip Trip);
        void Update(Trip obj);
        //DateTime CombineDateAndTime(DayOfWeek departureDay, TimeSpan departureTime);
    }
}