using BusBooking.Entity;

namespace BusBooking.Repository.IRepositories
{
    public interface IUserRepository : IRepository<User>
    {
        User? GetByUsername(string username);
    }
}
