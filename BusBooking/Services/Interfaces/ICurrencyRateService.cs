namespace BusBooking.Services.Interfaces
{
    public record CurrencyRates(string Base, IReadOnlyDictionary<string, decimal> Rates, DateTime AsOf);

    public interface ICurrencyRateService
    {
        Task<CurrencyRates> GetRatesAsync(CancellationToken cancellationToken = default);
    }
}
