using BusBookingDemo.Data;
using BusBookingDemo.Entity;
using BusBookingDemo.Repository.IRepositories;
using Microsoft.EntityFrameworkCore;
using Mono.TextTemplating;

namespace BusBookingDemo.Repository
{
    public class BookingRepository : Repository<Booking>, IBookingRepository
    {
        public BookingRepository(ApplicationDbContext context) : base(context) { }
        public void Update(Booking booking)
        {
            Items.Update(booking);
        }

        public IQueryable<Booking> GetBookingsByUserId(int userId)
        {
            return Context.Set<Booking>()
                           .Where(b => b.UserId == userId)
                           .Include(b => b.Trip)
                           .Include(b => b.SeatDetail)
                           .AsQueryable();
        }

        //public void ConfirmPayment(int orderId)
        //{
        //    var order = Items.Find(orderId);
        //if (order == null)
        //    throw new ArgumentException("Order not found.");

        //// Логика подтверждения оплаты
        //order.IsPaid = true;
        //Items.Update(order);
        //Context.SaveChanges();
        //}

        //public Order GetByCode(string orderCode)
        //{
        //    return Items.FirstOrDefault(o => o.OrderCode == orderCode);
        //}
    }

}


