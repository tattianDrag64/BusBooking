using BusBooking.Entities;
using BusBooking.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Repositories
{
    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        public OrderRepository(DbContext context) : base(context) { }

            //void AddSeat(int orderId, int seatId);
            //void RemoveSeat(int orderId, int seatId);
            //void ConfirmPayment(int orderId);

        public void Update(Order order)
        {
            Items.Update(order);
        }

        public IEnumerable<Order> GetOrdersByUser(int userId)
        {
            return Items.Where(o => o.UserId == userId).ToList();
        }

        public void AddSeat(int orderId, int seatId)
        {
            
        }

        public void RemoveSeat(int orderId, int seatId)
        {
            throw new NotImplementedException();
        }

        public void ConfirmPayment(int orderId)
        {
            throw new NotImplementedException();
        }

        public Order GetByCode(string orderCode)
        {
            return Items.FirstOrDefault(o => o.OrderCode == orderCode);
        }
    }

}


