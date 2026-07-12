using System.Security.Cryptography;
using System.Text;
using BusBooking.Services.Interfaces;

namespace BusBooking.Services
{
    // Static HttpClient shared across instances — ServiceManager is request-scoped,
    // so a per-instance HttpClient here would create/tear down a socket per request.
    public class PasswordBreachChecker : IPasswordBreachChecker
    {
        private static readonly HttpClient HttpClient = CreateHttpClient();

        private static HttpClient CreateHttpClient()
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri("https://api.pwnedpasswords.com/"),
                Timeout = TimeSpan.FromSeconds(5),
            };
            // Required by the HIBP API's usage policy.
            client.DefaultRequestHeaders.Add("User-Agent", "BusBooking-PasswordBreachChecker");
            return client;
        }

        public async Task<bool> IsPwnedAsync(string password, CancellationToken cancellationToken = default)
        {
            var sha1 = Convert.ToHexString(SHA1.HashData(Encoding.UTF8.GetBytes(password)));
            var prefix = sha1[..5];
            var suffix = sha1[5..];

            string body;
            try
            {
                // k-anonymity: only the first 5 hex chars of the SHA-1 hash are sent,
                // the API returns every suffix sharing that prefix so the full password
                // (and even its full hash) never leaves this process.
                body = await HttpClient.GetStringAsync($"range/{prefix}", cancellationToken);
            }
            catch (Exception)
            {
                // Fail open: an HIBP outage should not block user registration.
                return false;
            }

            foreach (var line in body.Split('\n'))
            {
                var parts = line.Split(':');
                if (parts.Length == 2 && parts[0].Trim().Equals(suffix, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
