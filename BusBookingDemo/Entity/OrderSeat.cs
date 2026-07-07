using BusBooking.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusBooking.Entities
{
    public class OrderSeat : BaseEntity
    {
        public int OrderId { get; set; }
        public int SeatDetailId { get; set; }
        public int TripId  { get; set; }

        public virtual Order Order { get; set; }
        public virtual SeatDetail SeatDetail { get; set; }
        public virtual Trip Trip { get; set; }

    }
}
