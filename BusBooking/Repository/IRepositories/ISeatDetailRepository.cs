using BusBooking.Entity;

namespace BusBooking.Repository.IRepositories
{
    public interface ISeatDetailRepository : IRepository<SeatDetail>
    {
        public IEnumerable<SeatDetail> GetSeatsByTrip(Guid tripId);
        public IEnumerable<Guid> HoldSeats(IEnumerable<Guid> seatIds, TimeSpan holdDuration);
        public void ConfirmSeat(Guid seatId);
        public void ConfirmSeats(IEnumerable<Guid> seatIds);
        public void ReleaseSeats(IEnumerable<Guid> seatIds);
        public IEnumerable<Guid> ReleaseExpiredHolds(Guid tripId);
    }
}
