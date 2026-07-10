using BusBooking.Data;
using BusBooking.Entity;
using BusBooking.Repository.IRepositories;

namespace BusBooking.Repository
{
    public class BusRepository(ApplicationDbContext context) : Repository<Bus>(context), IBusRepository
    {
        public int GetSeatsCount(Guid id)
        {
            var bus = Items.FirstOrDefault(b => b.Id == id)
                ?? throw new InvalidOperationException($"Bus {id} not found.");
            return bus.SeatsCount;
        }
    }
}
