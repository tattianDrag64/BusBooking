using BusBooking.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusBooking.Entities
{
    public class Route : BaseEntity
    {
        public string DepartureCity { get; set; }
        public string ArrivalCity { get; set; }
        public DayOfWeek DepartureDay { get; set; }
        public DayOfWeek ReturnDay { get; set; }
        public TimeSpan DepartureTime { get; set; }
        public TimeSpan ReturnTime { get; set; }
        public decimal Price { get; set; }

    }
}
