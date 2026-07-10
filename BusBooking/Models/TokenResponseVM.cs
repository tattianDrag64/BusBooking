namespace BusBooking.Models
{
    public class TokenResponseVM
    {
        public required string AccessToken { get; set; }
        public required string RefreshToken { get; set; }
    }
}