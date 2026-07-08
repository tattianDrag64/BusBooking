using BusBooking.Data;
using BusBooking.Entity;
using BusBooking.Repository.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Repository
{
    public class OrderRepository(ApplicationDbContext context) : Repository<Order>(context), IOrderRepository
    {

        public void Update(Order order)
        {
            Items.Update(order);
        }

        public IEnumerable<Order> GetOrdersByUser(Guid userId)
        {
            return [.. Items
                .Where(o => o.UserId == userId)
                .Include(o => o.Trip)
                .Include(o => o.OrderSeats)
                    .ThenInclude(os => os.SeatDetail)];
        }

        public Order? GetByCode(string orderCode)
        {
            return Items.FirstOrDefault(o => o.OrderCode == orderCode);
        }
    }
}
