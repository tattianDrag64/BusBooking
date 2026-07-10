using BusBooking.Services.Interfaces;

namespace BusBooking.Services.ServiceManager
{
    public interface IServiceManager
    {
        ITokenService TokenService { get; }
        IPasswordHasherService PasswordHasherService { get; }
    }
}