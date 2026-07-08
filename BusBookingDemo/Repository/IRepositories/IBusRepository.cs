using BusBookingDemo.Entity;
namespace BusBookingDemo.Repository.IRepositories
{
    public interface IBusRepository : IRepository<Bus>
    {
        public int GetSeatsCount(Guid id);
        void Update(Bus obj);
    }
}
