using BusBookingDemo.Entity;

namespace BusBookingDemo.Repository.IRepositories
{
    public interface IBookingRepository : IRepository<Booking>
    {
        //IQueryable<Booking> Bookings { get; }
        public IQueryable<Booking> GetBookingsByUserId(int userId);
    }
}

