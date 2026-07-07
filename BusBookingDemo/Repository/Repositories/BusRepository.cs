using BusBooking.Entities;
using BusBooking.Repositories;
using BusBooking.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Repositories
{
    public class BusRepository : Repository<Bus>, IBusRepository
    {
        public BusRepository(DbContext context) : base(context) { }
    
        public void Update(Bus bus)
        {
            Items.Update(bus);
        }
    }
}
