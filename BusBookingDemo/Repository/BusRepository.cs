using BusBookingDemo.Data;
using BusBookingDemo.Entity;
using BusBookingDemo.Repository.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace BusBookingDemo.Repository
{
    public class BusRepository : Repository<Bus>, IBusRepository
    {
        public BusRepository(ApplicationDbContext context) : base(context) { }

        public int GetSeatsCount(int id)
        {
            var bus = Items.FirstOrDefault(b => b.Id == id); // Ищем автобус с указанным ID
            return bus.SeatsCount;
        }
        public void Update(Bus bus)
        {
            Items.Update(bus);
        }
    }
}
