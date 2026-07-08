using BusBookingDemo.Data;
using BusBookingDemo.Entity;
using BusBookingDemo.Repository.IRepositories;

namespace BusBookingDemo.Repository
{
    public class BusRepository(ApplicationDbContext context) : Repository<Bus>(context), IBusRepository
    {
        public int GetSeatsCount(Guid id)
        {
            var bus = Items.FirstOrDefault(b => b.Id == id)
                ?? throw new InvalidOperationException($"Bus {id} not found.");
            return bus.SeatsCount;
        }
        public void Update(Bus bus)
        {
            Items.Update(bus);
        }
    }
}
