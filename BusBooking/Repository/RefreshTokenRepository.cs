using BusBooking.Data;
using BusBooking.Entity;
using BusBooking.Repository.IRepositories;

namespace BusBooking.Repository
{
    public class RefreshTokenRepository(ApplicationDbContext context) : Repository<RefreshToken>(context), IRefreshTokenRepository
    {
        public RefreshToken? GetByToken(string token)
        {
            return Items.FirstOrDefault(rt => rt.Token == token);
        }
    }
}