using BusBooking.Data;
using BusBooking.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private DbContext Context;
        public IBusRepository Bus { get; private set; }
        public IOrderRepository Order { get; private set; }
        public IOrderSeatRepository OrderSeat { get; private set; }
        public IRouteRepository Route { get; private set; }
        public ISeatDetailRepository SeatDetail { get; private set; }
        public ITripRepository Trip { get; private set; }
        public IUserRepository User { get; private set; }

        public UnitOfWork(BusBookingDbContext context)
        {
            Context = context;
            Bus = new BusRepository(Context);
        }

        public void Save()
        {
            throw new NotImplementedException();
        }
    }
}