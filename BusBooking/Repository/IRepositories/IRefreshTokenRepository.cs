using BusBooking.Entity;

namespace BusBooking.Repository.IRepositories
{
    public interface IRefreshTokenRepository : IRepository<RefreshToken>
    {
        RefreshToken? GetByToken(string token);

        // Revokes every still-active token for a user — used when a revoked/rotated
        // token is presented again, a signal that it was stolen (see AuthController.Refresh).
        void RevokeAllForUser(Guid userId);
    }
}