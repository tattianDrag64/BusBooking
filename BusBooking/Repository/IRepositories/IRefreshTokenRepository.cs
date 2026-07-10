using BusBooking.Entity;

namespace BusBooking.Repository.IRepositories
{
    public interface IRefreshTokenRepository : IRepository<RefreshToken>
    {
        RefreshToken? GetByToken(string token);
    }
}