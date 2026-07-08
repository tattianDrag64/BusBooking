using BusBooking.Entity;

namespace BusBooking.Repository.IRepositories
{
    public interface IOrderSeatRepository : IRepository<OrderSeat>
    {
        IEnumerable<OrderSeat> GetByOrderId(Guid orderId);
        void AddSeats(IEnumerable<OrderSeat> seats);
        void Update(OrderSeat obj);
    }
}
