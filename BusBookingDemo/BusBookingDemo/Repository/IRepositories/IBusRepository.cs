using BusBookingDemo.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusBookingDemo.Repository.IRepositories
{
    public interface IBusRepository : IRepository<Bus>
    {
        public int GetSeatsCount(int id);
        void Update(Bus obj);
    }
}
