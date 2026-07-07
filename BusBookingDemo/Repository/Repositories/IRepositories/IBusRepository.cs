using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusBooking.Entities;

namespace BusBooking.Repositories.IRepositories
{
    public interface IBusRepository : IRepository<Bus>
    {
        void Update(Bus obj);
    }
}
