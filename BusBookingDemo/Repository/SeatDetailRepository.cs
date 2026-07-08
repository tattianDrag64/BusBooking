using BusBookingDemo.Data;
using BusBookingDemo.Entity;
using BusBookingDemo.Repository.IRepositories;
using Microsoft.EntityFrameworkCore;
using System;

namespace BusBookingDemo.Repository
{
    public class SeatDetailRepository(ApplicationDbContext context) : Repository<SeatDetail>(context), ISeatDetailRepository
    {

        //public bool CheckSeatStatus(int seatNumber, int busId)
        //{
        //    var seat = Items.FirstOrDefault(sd => sd.SeatNumber == seatNumber && sd.BusId == busId);
        //    return seat != null && seat.IsReserved;
        //}

        //public IEnumerable<SeatDetail> GetAvaiableSeatDetail(int busId)
        //{
        //    return Items.Where(sd => sd.BusId == busId && sd.IsReserved == false).ToList();
        //}

        public IEnumerable<SeatDetail> GetSeatsByTrip(Guid tripId)
        {
            return [.. Items.Where(s => s.TripId == tripId && !s.IsOccupied)];
        }

        public void ReserveSeat(Guid seatId)
        {
            var seat = Items.FirstOrDefault(sd => sd.Id == seatId);
            if (seat != null && !seat.IsOccupied)
            {
                seat.IsOccupied = true;
                Items.Update(seat);
                Context.SaveChanges();
            }
            ;
        }
    }

}
