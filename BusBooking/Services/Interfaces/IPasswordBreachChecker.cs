namespace BusBooking.Services.Interfaces
{
    public interface IPasswordBreachChecker
    {
        // Checks the password against the Have I Been Pwned Pwned Passwords list
        // (k-anonymity model — only a SHA-1 hash prefix ever leaves the server).
        Task<bool> IsPwnedAsync(string password, CancellationToken cancellationToken = default);
    }
}
