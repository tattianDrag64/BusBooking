namespace BusBooking.Repositories.IRepositories
{
    public interface IUnitOfWork
    {
        IBusRepository Bus { get; }
        IOrderRepository Order { get; }
        IOrderSeatRepository OrderSeat { get; } 
        IRouteRepository Route { get; }
        ISeatDetailRepository SeatDetail { get; }
        ITripRepository Trip { get; }
        IUserRepository User { get; }
        void Save();
    }
}
