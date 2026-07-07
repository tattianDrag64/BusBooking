using BusBooking.Entities;

namespace BusBooking.Repositories.IRepositories
{
    public interface IUserRepository : IRepository<User>
    {
        User GetByUsername(string username);
        void Update(User obj);
    }
}
