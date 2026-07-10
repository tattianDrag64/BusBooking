namespace BusBooking.Entity
{
    public class RefreshToken : BaseEntity
    {
        public required Guid UserId { get; set; }
        public required string Token { get; set; }
        public required DateTime Expires { get; set; }
        public bool IsRevoked { get; set; } = false;
        public virtual User User { get; set; } = null!;
    }
}