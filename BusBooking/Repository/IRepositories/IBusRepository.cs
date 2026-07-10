using BusBooking.Entity;
namespace BusBooking.Repository.IRepositories
{
    public interface IBusRepository : IRepository<Bus>
    {
        public int GetSeatsCount(Guid id);
    }
}