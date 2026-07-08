namespace BusBookingDemo.Repository.IRepositories
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

        void Save();
    }
}
