
using BusBookingDemo.Data;
using BusBookingDemo.Entity;
using BusBookingDemo.Repository.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace BusBookingDemo.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext Context;
        public IBusRepository Bus { get; private set; }
        public ISeatDetailRepository SeatDetail { get; private set; }
        public ITripRepository Trip { get; private set; }
        public IUserRepository User { get; private set; }
        public IOrderRepository Order { get; private set; }
        public IOrderSeatRepository OrderSeat { get; private set; }
        public IRouteRepository Route { get; private set; }

        public UnitOfWork(ApplicationDbContext context)
        {
            Context = context;
            Bus = new BusRepository(Context);
            User = new UserRepository(Context);
            SeatDetail = new SeatDetailRepository(Context);
            Trip = new TripRepository(Context);
            Order = new OrderRepository(Context);
            OrderSeat = new OrderSeatRepository(Context);
            Route = new RouteRepository(Context);
        }

        public void Save()
        {
            Context.SaveChanges();
        }
    }
}