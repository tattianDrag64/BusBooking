using BusBooking.Services.Interfaces;

namespace BusBooking.Services.ServiceManager
{
    public class ServiceManager(IConfiguration configuration) : IServiceManager
    {
        private readonly Lazy<ITokenService> _tokenService = new(() => new TokenService(configuration));
        private readonly Lazy<IPasswordHasherService> _passwordHasherService = new(() => new PasswordHasherService());
        private readonly Lazy<IPaymentService> _paymentService = new(() => new PaymentService(configuration));
        private readonly Lazy<IPasswordBreachChecker> _passwordBreachChecker = new(() => new PasswordBreachChecker());
        private readonly Lazy<ICurrencyRateService> _currencyRateService = new(() => new CurrencyRateService());

        public ITokenService TokenService => _tokenService.Value;
        public IPasswordHasherService PasswordHasherService => _passwordHasherService.Value;
        public IPaymentService PaymentService => _paymentService.Value;
        public IPasswordBreachChecker PasswordBreachChecker => _passwordBreachChecker.Value;
        public ICurrencyRateService CurrencyRateService => _currencyRateService.Value;
    }
}