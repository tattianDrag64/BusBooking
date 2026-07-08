using BusBooking.Data;
using BusBooking.Entity;
using BusBooking.Repository.IRepositories;

namespace BusBooking.Repository
{
    public class OrderSeatRepository(ApplicationDbContext context) : Repository<OrderSeat>(context), IOrderSeatRepository
    {

        public IEnumerable<OrderSeat> GetByOrderId(Guid orderId)
        {
            return [.. Items.Where(os => os.OrderId == orderId)];
        }

        public void AddSeats(IEnumerable<OrderSeat> seats)
        {
            Items.AddRange(seats);
        }

        public void Update(OrderSeat obj)
        {
            Items.Update(obj);
        }
    }
}
