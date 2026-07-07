using BusBooking.Entities;

namespace BusBooking.Repositories.IRepositories
{
    public interface ISeatDetailRepository : IRepository<SeatDetail>
    {
        bool CheckSeatStatus(int seatNumber, int busIs);
        IEnumerable<SeatDetail> GetAvaiableSeatDetail(int busId);
        void UpdateSeatReservationStatus(int seatId, bool isReserved);
    }
}
