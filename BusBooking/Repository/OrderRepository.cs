using BusBooking.Data;
using BusBooking.Entity;
using BusBooking.Repository.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Repository
{
    public class OrderRepository(ApplicationDbContext context) : Repository<Order>(context), IOrderRepository
    {
        public IEnumerable<Order> GetOrdersByUser(Guid userId)
        {
            return [.. Items
                .Where(o => o.UserId == userId)
                .Include(o => o.Trip)
                .Include(o => o.OrderSeats)
                    .ThenInclude(os => os.SeatDetail)];
        }

        // Admin-only listing (BookingsController.AllBookings) — same shape as
        // GetOrdersByUser plus User, not scoped to one user.
        public IEnumerable<Order> GetAllWithDetails()
        {
            return [.. Items
                .Include(o => o.User)
                .Include(o => o.Trip)
                .Include(o => o.OrderSeats)
                    .ThenInclude(os => os.SeatDetail)
                .OrderByDescending(o => o.CreatedAt)];
        }

        public Order? GetByCode(string orderCode)
        {
            return Items.FirstOrDefault(o => o.OrderCode == orderCode);
        }

        // Tracked, with Trip (for the cancellation deadline check) and OrderSeats (to
        // release the booked seats) — used by BookingsController's cancel endpoints.
        public Order? GetByIdWithDetails(Guid id)
        {
            return Items
                .Include(o => o.Trip)
                .Include(o => o.OrderSeats)
                .FirstOrDefault(o => o.Id == id);
        }

        public IEnumerable<Order> GetByIds(IEnumerable<Guid> ids, bool tracked = false)
        {
            var idList = ids as ICollection<Guid> ?? [.. ids];
            if (idList.Count == 0)
            {
                return [];
            }
            IQueryable<Order> query = tracked ? Items : Items.AsNoTracking();
            return [.. query.Where(o => idList.Contains(o.Id))];
        }
    }
}