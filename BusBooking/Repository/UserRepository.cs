using BusBooking.Data;
using BusBooking.Entity;
using BusBooking.Repository.IRepositories;

namespace BusBooking.Repository
{
    public class UserRepository(ApplicationDbContext context) : Repository<User>(context), IUserRepository
    {
        public User? GetByUsername(string username)
        {
            return Items.FirstOrDefault(u => u.Username == username);
        }

    }
}
