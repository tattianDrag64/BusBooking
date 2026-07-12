using BusBooking.Repository.IRepositories;

namespace BusBooking.Repository.UnitOfWork
{
    public interface IUnitOfWork
    {
        IBusRepository Bus { get; }
        ISeatDetailRepository SeatDetail { get; }
        ITripRepository Trip { get; }
        IUserRepository User { get; }
        IOrderRepository Order { get; }
        IOrderSeatRepository OrderSeat { get; }
        IRouteRepository Route { get; }
        IRefreshTokenRepository RefreshToken { get; }
        IScheduleRepository Schedule { get; }

        void Save();
    }
}
