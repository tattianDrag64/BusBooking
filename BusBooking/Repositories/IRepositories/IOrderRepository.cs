using BusBooking.Entities;

namespace BusBooking.Repositories.IRepositories
{
    public interface IOrderRepository : IRepository<Order>
    {
        void AddSeat(int orderId, int seatId);
        void RemoveSeat(int orderId, int seatId);
        void ConfirmPayment(int orderId);
        void Update(Order obj);
        public IEnumerable<Order> GetOrdersByUser(int userId);
        Order GetByCode(string orderCode);
    }
}
