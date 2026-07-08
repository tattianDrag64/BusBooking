using BusBookingDemo.Entity;

namespace BusBookingDemo.Repository.IRepositories
{
    public interface IOrderRepository : IRepository<Order>
    {
        void Update(Order obj);
        IEnumerable<Order> GetOrdersByUser(Guid userId);
        Order? GetByCode(string orderCode);
    }
}
