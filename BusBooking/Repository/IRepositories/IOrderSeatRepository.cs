using BusBooking.Entity;

namespace BusBooking.Repository.IRepositories
{
    public interface IOrderSeatRepository : IRepository<OrderSeat>
    {
        IEnumerable<OrderSeat> GetByOrderId(Guid orderId);
        IEnumerable<OrderSeat> GetBySeatDetailIds(IEnumerable<Guid> seatDetailIds);
    }
}
