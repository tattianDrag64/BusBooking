namespace BusBookingDemo.Repository.IRepositories
{
    public interface IUnitOfWork
    {
        IBusRepository Bus { get; }
        IBookingRepository Booking { get; }
        ISeatDetailRepository SeatDetail { get; }
        ITripRepository Trip { get; }
        IUserRepository User { get; }
        //ICartRepository Cart { get; }
        void Save();
    }
}
