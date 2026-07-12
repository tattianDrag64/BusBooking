using BusBooking.Entity;

namespace BusBooking.Repository.IRepositories
{
    public interface IOrderRepository : IRepository<Order>
    {
        IEnumerable<Order> GetOrdersByUser(Guid userId);
        IEnumerable<Order> GetAllWithDetails();
        Order? GetByCode(string orderCode);
        IEnumerable<Order> GetByIds(IEnumerable<Guid> ids, bool tracked = false);
        Order? GetByIdWithDetails(Guid id);
    }
}
