using BusBooking.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusBooking.Entities
{
    public class SeatDetail : BaseEntity
    {
        public int SeatNumber { get; set; }
        public bool IsReserved { get; set; } = false;

        public int BusId { get; set; }
        public virtual Bus Bus { get; set; }


    }
}
