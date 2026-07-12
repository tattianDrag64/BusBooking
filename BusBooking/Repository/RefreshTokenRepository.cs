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

        public void RevokeAllForUser(Guid userId)
        {
            var tokens = Items.Where(rt => rt.UserId == userId && !rt.IsRevoked).ToList();
            foreach (var token in tokens)
            {
                token.IsRevoked = true;
            }
            if (tokens.Count > 0)
            {
                Context.SaveChanges();
            }
        }
    }
}