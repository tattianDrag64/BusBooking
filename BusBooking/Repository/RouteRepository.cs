using BusBooking.Data;
using BusBooking.Entity;
using BusBooking.Repository.IRepositories;

namespace BusBooking.Repository
{
    public class RouteRepository(ApplicationDbContext context) : Repository<RouteInfo>(context), IRouteRepository
    {
    }
}
