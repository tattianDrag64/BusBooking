using BusBooking.Data;
using BusBooking.Repository.IRepositories;

namespace BusBooking.Repository.UnitOfWork
{
    public class UnitOfWork(ApplicationDbContext context) : IUnitOfWork
    {
        public IBusRepository Bus { get; } = new BusRepository(context);
        public ISeatDetailRepository SeatDetail { get; } = new SeatDetailRepository(context);
        public ITripRepository Trip { get; } = new TripRepository(context);
        public IUserRepository User { get; } = new UserRepository(context);
        public IOrderRepository Order { get; } = new OrderRepository(context);
        public IOrderSeatRepository OrderSeat { get; } = new OrderSeatRepository(context);
        public IRouteRepository Route { get; } = new RouteRepository(context);
        public IRefreshTokenRepository RefreshToken { get; } = new RefreshTokenRepository(context);
        public void Save()
        {
            context.SaveChanges();
        }
    }
}