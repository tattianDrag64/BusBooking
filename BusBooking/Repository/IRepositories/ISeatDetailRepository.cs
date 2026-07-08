using BusBooking.Entity;

namespace BusBooking.Repository.IRepositories
{
    public interface ISeatDetailRepository : IRepository<SeatDetail>
    {
        //bool CheckSeatStatus(int seatNumber, int busId);
        //IEnumerable<SeatDetail> GetAvaiableSeatDetail(int busId);
        //void UpdateSeatReservationStatus(int seatId);
        //public IEnumerable<SeatDetail> GetSeatsByBus(int busId);
        public IEnumerable<SeatDetail> GetSeatsByTrip(Guid tripId);
        public void ReserveSeat(Guid seatId);
    }
}
