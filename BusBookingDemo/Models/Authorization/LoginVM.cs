namespace BusBookingProject.Models.ViewModels.Authorization
{
    public class LoginVM
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }
        public string ReturnUrl { get; set; }
    }
}
