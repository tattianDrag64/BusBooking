using BusBooking.Services.Interfaces;

namespace BusBooking.Services.ServiceManager
{
    public class ServiceManager(ITokenService tokenService, IPasswordHasherService passwordHasherService) : IServiceManager
    {
        private readonly ITokenService _tokenService = tokenService;
        private readonly IPasswordHasherService _passwordHasherService = passwordHasherService;

        public ITokenService TokenService => _tokenService;

        public IPasswordHasherService PasswordHasherService => _passwordHasherService;
    }
}