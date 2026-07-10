using BusBooking.Entity;

namespace BusBooking.Repository.IRepositories
{
    public interface IOrderRepository : IRepository<Order>
    {
        IEnumerable<Order> GetOrdersByUser(Guid userId);
        Order? GetByCode(string orderCode);
    }
}
