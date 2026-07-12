using BusBooking.Services.Interfaces;

namespace BusBooking.Services.ServiceManager
{
    public interface IServiceManager
    {
        ITokenService TokenService { get; }
        IPasswordHasherService PasswordHasherService { get; }
        IPaymentService PaymentService { get; }
        IPasswordBreachChecker PasswordBreachChecker { get; }
        ICurrencyRateService CurrencyRateService { get; }

    }
}