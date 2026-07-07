
using BusBookingDemo.Data;
using BusBookingDemo.Entity;
using BusBookingDemo.Repository.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace BusBookingDemo.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private ApplicationDbContext Context;
        public IBusRepository Bus { get; private set; }
        public IBookingRepository Booking { get; private set; }
        //public IOrderSeatRepository OrderSeat { get; private set; }
        //public IDirectionRepository Direction { get; private set; }
        public ISeatDetailRepository SeatDetail { get; private set; }
        public ITripRepository Trip { get; private set; }
        public IUserRepository User { get; private set; }
        // public ICartRepository Cart {  get; private set; }

        public UnitOfWork(ApplicationDbContext context)
        {
            Context = context;
            Bus = new BusRepository(Context);
            Booking = new BookingRepository(Context);
            //OrderSeat = new OrderSeatRepository(Context);
            //Direction = new DirectionRepository(Context);
            User = new UserRepository(Context);
            SeatDetail = new SeatDetailRepository(Context);
            Trip = new TripRepository(Context);
            //Cart = new CartRepository(Context);
        }

        public void Save()
        {
            Context.SaveChanges();
        }
    }
}