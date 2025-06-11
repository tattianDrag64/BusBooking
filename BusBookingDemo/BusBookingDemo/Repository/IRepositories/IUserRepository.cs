using BusBookingDemo.Entity;

namespace BusBookingDemo.Repository.IRepositories
{
    public interface IUserRepository : IRepository<User>
    {
        //User GetByUsername(string username);

        string HashPassword(string password);
        bool VerifyPassword(string inputPassword, string storedHash);
        void Update(User obj);
    }
}
