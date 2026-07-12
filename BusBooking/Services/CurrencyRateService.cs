using System.Text.Json;
using BusBooking.Services.Interfaces;

namespace BusBooking.Services
{
    // Static HttpClient + cache shared across instances — ServiceManager is
    // request-scoped, so per-instance state here would refetch on every request.
    public class CurrencyRateService : ICurrencyRateService
    {
        private const string BaseCurrency = "EUR";
        private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(6);
        private static readonly SemaphoreSlim RefreshLock = new(1, 1);
        private static readonly HttpClient HttpClient = new()
        {
            BaseAddress = new Uri("https://open.er-api.com/"),
            Timeout = TimeSpan.FromSeconds(5),
        };

        private static CurrencyRates? _cached;

        public async Task<CurrencyRates> GetRatesAsync(CancellationToken cancellationToken = default)
        {
            if (_cached != null && DateTime.UtcNow - _cached.AsOf < CacheDuration)
            {
                return _cached;
            }

            await RefreshLock.WaitAsync(cancellationToken);
            try
            {
                // Another request may have refreshed the cache while this one waited.
                if (_cached != null && DateTime.UtcNow - _cached.AsOf < CacheDuration)
                {
                    return _cached;
                }

                try
                {
                    var response = await HttpClient.GetStreamAsync($"v6/latest/{BaseCurrency}", cancellationToken);
                    var payload = await JsonSerializer.DeserializeAsync<ExchangeRateResponse>(
                        response,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
                        cancellationToken);

                    if (payload?.Rates is { Count: > 0 })
                    {
                        _cached = new CurrencyRates(BaseCurrency, payload.Rates, DateTime.UtcNow);
                    }
                }
                catch (Exception)
                {
                    // Fail open: keep serving the last known rates (even if stale) rather
                    // than breaking price display because the external API is down. If
                    // there's no cache at all yet, fall back to base-currency-only.
                }

                return _cached ??= new CurrencyRates(
                    BaseCurrency,
                    new Dictionary<string, decimal> { [BaseCurrency] = 1m },
                    DateTime.UtcNow);
            }
            finally
            {
                RefreshLock.Release();
            }
        }

        private class ExchangeRateResponse
        {
            public Dictionary<string, decimal> Rates { get; set; } = [];
        }
    }
}
