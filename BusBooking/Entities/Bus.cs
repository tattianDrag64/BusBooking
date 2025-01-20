using BusBooking.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusBooking.Entities
{
    public class Bus : BaseEntity
    {
        public string BusNumber { get; set; }
        public int SeatsCount { get; set; }
        public int RouteId { get; set; }

        public virtual Route Route { get; set; }
    }
}
