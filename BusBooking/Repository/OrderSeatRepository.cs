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

        public IEnumerable<OrderSeat> GetBySeatDetailIds(IEnumerable<Guid> seatDetailIds)
        {
            var ids = seatDetailIds as ICollection<Guid> ?? [.. seatDetailIds];
            return ids.Count == 0 ? [] : [.. Items.Where(os => ids.Contains(os.SeatDetailId))];
        }
    }
}
