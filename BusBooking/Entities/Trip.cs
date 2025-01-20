using BusBooking.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace BusBooking.Entities
{
    public class Trip : BaseEntity
    {
        public int ScheduleId { get; set; }
        public DateTime DepartureDate { get; set; } 
        public DateTime ArrivalDate { get; set; }  
        public bool IsReturnTrip { get; set; }
        public int RouteId { get; set; }

        public virtual Route Route { get; set; }
    }
}
