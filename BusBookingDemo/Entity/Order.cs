using BusBooking.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace BusBooking.Entities
{
    public class Order : BaseEntity
    {
        public string OrderCode { get; set; }
        public int UserId { get; set; }
        public int RouteId { get; set; }
        public int TripId { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual User User { get; set; }
        public virtual Route Route { get; set; }
    }
}
