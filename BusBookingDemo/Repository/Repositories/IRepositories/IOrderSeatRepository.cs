using BusBooking.Entities;

namespace BusBooking.Repositories.IRepositories
{
    public interface IOrderSeatRepository : IRepository<OrderSeat>
    {
        IEnumerable<OrderSeat> GetByOrderId(int orderId);
        void AddSeats(IEnumerable<OrderSeat> obj);
        void Update(OrderSeat obj);
    }
}
