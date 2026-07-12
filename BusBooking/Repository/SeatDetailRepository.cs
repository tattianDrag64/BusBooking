using BusBooking.Data;
using BusBooking.Entity;
using BusBooking.Repository.IRepositories;
using Microsoft.EntityFrameworkCore;
using System;

namespace BusBooking.Repository
{
    public class SeatDetailRepository(ApplicationDbContext context) : Repository<SeatDetail>(context), ISeatDetailRepository
    {
        public IEnumerable<SeatDetail> GetSeatsByTrip(Guid tripId)
        {
            return [.. Items.Where(s => s.TripId == tripId && !s.IsOccupied)];
        }

        // Places a temporary hold on one or more seats for a single order — all-or-nothing:
        // if any requested seat is unavailable, none of them are held (a partial hold would
        // leave an order half-booked). Returns the ids of seats that turned out unavailable
        // (empty means every seat was successfully held).
        public IEnumerable<Guid> HoldSeats(IEnumerable<Guid> seatIds, TimeSpan holdDuration)
        {
            var ids = seatIds as ICollection<Guid> ?? [.. seatIds];
            var now = DateTime.UtcNow;
            var seats = Items.Where(sd => ids.Contains(sd.Id)).ToList();

            var unavailable = seats
                .Where(s => s.IsOccupied || (s.IsReserved && s.ReservedUntil > now))
                .Select(s => s.Id)
                .Concat(ids.Except(seats.Select(s => s.Id))) // requested ids that don't even exist
                .ToList();

            if (unavailable.Count > 0)
            {
                return unavailable;
            }

            foreach (var seat in seats)
            {
                seat.IsReserved = true;
                seat.ReservedUntil = now.Add(holdDuration);
            }
            Context.SaveChanges();
            return [];
        }

        // Called once payment is confirmed — turns a temporary hold into a permanent booking.
        public void ConfirmSeat(Guid seatId) => ConfirmSeats([seatId]);

        // Batched version — one SELECT + one SaveChanges for the whole order, instead of
        // per-seat round trips when an order books multiple seats (see PaymentsController).
        public void ConfirmSeats(IEnumerable<Guid> seatIds)
        {
            var ids = seatIds as ICollection<Guid> ?? [.. seatIds];
            if (ids.Count == 0)
            {
                return;
            }

            var seats = Items.Where(sd => ids.Contains(sd.Id)).ToList();
            foreach (var seat in seats)
            {
                seat.IsOccupied = true;
                seat.IsReserved = false;
                seat.ReservedUntil = null;
            }
            Context.SaveChanges();
        }

        // Frees seats booked by a cancelled/refunded order — opposite of ConfirmSeats.
        public void ReleaseSeats(IEnumerable<Guid> seatIds)
        {
            var ids = seatIds as ICollection<Guid> ?? [.. seatIds];
            if (ids.Count == 0)
            {
                return;
            }

            var seats = Items.Where(sd => ids.Contains(sd.Id)).ToList();
            foreach (var seat in seats)
            {
                seat.IsOccupied = false;
                seat.IsReserved = false;
                seat.ReservedUntil = null;
            }
            Context.SaveChanges();
        }

        // Lazily releases holds whose TTL has passed (checked on read, no background job).
        // Returns the seat ids that were released so callers can also expire their orders.
        public IEnumerable<Guid> ReleaseExpiredHolds(Guid tripId)
        {
            var now = DateTime.UtcNow;
            var expired = Items
                .Where(sd => sd.TripId == tripId && sd.IsReserved && !sd.IsOccupied && sd.ReservedUntil <= now)
                .ToList();

            foreach (var seat in expired)
            {
                seat.IsReserved = false;
                seat.ReservedUntil = null;
            }

            if (expired.Count > 0)
            {
                Context.SaveChanges();
            }

            return expired.Select(s => s.Id);
        }
    }

}
