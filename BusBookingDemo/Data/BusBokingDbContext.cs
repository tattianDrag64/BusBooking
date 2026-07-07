using BusBooking.Entities;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Data
{
    public class BusBookingDbContext : DbContext
    {
        public BusBookingDbContext(DbContextOptions<BusBookingDbContext> options) : base(options) { }

        public BusBookingDbContext()
        {
        }

        public DbSet<User> User { get; set; }
        public DbSet<Bus> Buses { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderSeat> OrderSeats { get; set; }
        public DbSet<Trip> Trips { get; set; }
        public DbSet<SeatDetail> SeatDetals { get; set; }
        public DbSet<Entities.Route> Routes { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region User Config 
            modelBuilder.Entity<User>().HasKey(x => x.Id);

            modelBuilder.Entity<User>().HasData(new User()
            {
                Id = 1,
                FirstName = "Tatiana",
                LastName = "Tat",
                Username = "admin",
                Password = "1234",
                Email = "xyz@gmail.com",
                Phone = "+0987654321"
            });

            #endregion 

            #region Bus Config

            modelBuilder.Entity<Bus>().HasKey(x => x.Id);
            modelBuilder.Entity<Bus>().HasOne(r => r.Route)
                .WithMany()
                .HasForeignKey(f => f.RouteId)
                .OnDelete(DeleteBehavior.Restrict);

            #endregion

            #region Order Config

            modelBuilder.Entity<Order>().HasKey(x => x.Id);

            modelBuilder.Entity<Order>().HasOne(r => r.Route).
                WithMany().
                HasForeignKey(f => f.RouteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>().HasOne(u => u.User)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            #endregion

            #region OrderSeat Config 

            modelBuilder.Entity<OrderSeat>().HasKey(x => x.Id);

            modelBuilder.Entity<OrderSeat>().HasOne(o => o.Order)
                .WithMany()
                .HasForeignKey(f => f.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderSeat>().HasOne(o => o.SeatDetail)
                .WithMany()
                .HasForeignKey(f => f.SeatDetailId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderSeat>().HasOne(t => t.Trip)
                .WithMany()
                .HasForeignKey(f => f.TripId)
                .OnDelete(DeleteBehavior.Restrict);

            #endregion


            #region Route Config 

            modelBuilder.Entity<Entities.Route>().HasKey(x => x.Id);

            #endregion

            #region SeatDetail Config 

            modelBuilder.Entity<SeatDetail>().HasKey(x => x.Id);

            modelBuilder.Entity<SeatDetail>().HasOne(b => b.Bus)
                .WithMany()
                .HasForeignKey(f => f.BusId)
                .OnDelete(DeleteBehavior.Restrict);

            #endregion

            #region Trip Config 

            modelBuilder.Entity<Trip>().HasKey(x => x.Id);

            modelBuilder.Entity<Trip>().HasOne(b => b.Route)
                .WithMany()
                .HasForeignKey(f => f.RouteId)
                .OnDelete(DeleteBehavior.Restrict);

            #endregion
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
             .UseSqlServer(@"Server=localhost;Database=BusBookingSystemDB;Trusted_Connection=True;TrustServerCertificate=True;");
        }
    }
}
