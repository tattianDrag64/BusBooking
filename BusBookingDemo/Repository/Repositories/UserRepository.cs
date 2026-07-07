namespace BusBooking.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public User GetByUsername(string username)
        {
            throw new NotImplementedException();
        }

        public void Update(User obj)
        {
            throw new NotImplementedException();
        }
    }
}
