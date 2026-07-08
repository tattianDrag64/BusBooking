using BusBooking.Entity;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<User> User { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderSeat> OrderSeats { get; set; }
        public DbSet<Trip> Trips { get; set; }
        public DbSet<SeatDetail> SeatDetals { get; set; }
        public DbSet<Bus> Buses { get; set; }
        public DbSet<RouteInfo> Routes { get; set; }

        private static readonly Guid SeedRouteId = Guid.Parse("a0000000-0000-0000-0000-000000000001");
        private static readonly Guid SeedBus1Id = Guid.Parse("b0000000-0000-0000-0000-000000000001");
        private static readonly Guid SeedBus2Id = Guid.Parse("b0000000-0000-0000-0000-000000000002");
        private static readonly Guid SeedBus3Id = Guid.Parse("b0000000-0000-0000-0000-000000000003");
        private static readonly Guid SeedUser1Id = Guid.Parse("c0000000-0000-0000-0000-000000000001");
        private static readonly Guid SeedUser2Id = Guid.Parse("c0000000-0000-0000-0000-000000000002");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RouteInfo>().HasData(
                    new RouteInfo
                    {
                        Id = SeedRouteId,
                        DepartureCity = "Sofia",
                        ArrivalCity = "Berlin",
                        DepartureDay = DayOfWeek.Monday,
                        ReturnDay = DayOfWeek.Tuesday,
                        DepartureTime = TimeSpan.Parse("08:00:00"),
                        ReturnTime = TimeSpan.Parse("18:00:00"),
                        Price = 45.00m
                    });

            modelBuilder.Entity<Bus>().HasData(
                    new Bus { Id = SeedBus1Id, BusNumber = "234AA", SeatsCount = 24, RouteId = SeedRouteId },
                    new Bus { Id = SeedBus2Id, BusNumber = "211DA", SeatsCount = 34, RouteId = SeedRouteId },
                    new Bus { Id = SeedBus3Id, BusNumber = "232AC", SeatsCount = 20, RouteId = SeedRouteId }
                    );

            modelBuilder.Entity<User>().HasData(
                    new User
                    {
                        Id = SeedUser1Id,
                        FirstName = "tatiana",
                        LastName = "tat",
                        Username = "tati",
                        PasswordHash = "1234",
                        Email = "xyz@mail.com",
                        Phone = "12345",
                        Role = "Admin"
                    },
                    new User
                    {
                        Id = SeedUser2Id,
                        FirstName = "anna",
                        LastName = "aniina",
                        Username = "anni",
                        PasswordHash = "1234",
                        Email = "xyz1@mail.com",
                        Phone = "12345",
                        Role = "Customer"
                    }
                    );

            // Order
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Route)
                .WithMany()
                .HasForeignKey(o => o.RouteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Trip)
                .WithMany()
                .HasForeignKey(o => o.TripId)
                .OnDelete(DeleteBehavior.Restrict);

            // OrderSeat
            modelBuilder.Entity<OrderSeat>()
                .HasOne(os => os.Order)
                .WithMany(o => o.OrderSeats)
                .HasForeignKey(os => os.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderSeat>()
                .HasOne(os => os.SeatDetail)
                .WithMany()
                .HasForeignKey(os => os.SeatDetailId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderSeat>()
                .HasOne(os => os.Trip)
                .WithMany()
                .HasForeignKey(os => os.TripId)
                .OnDelete(DeleteBehavior.Restrict);

            // Место не может участвовать в двух активных заказах одновременно
            modelBuilder.Entity<OrderSeat>()
                .HasIndex(os => os.SeatDetailId)
                .IsUnique();

            // Trip
            modelBuilder.Entity<Trip>()
                .HasOne(t => t.Bus)
                .WithMany()
                .HasForeignKey(t => t.BusId)
                .OnDelete(DeleteBehavior.Restrict); // Автобус может быть удален с каскадным удалением поездок

            modelBuilder.Entity<Trip>()
                .HasOne(t => t.Route)
                .WithMany()
                .HasForeignKey(t => t.RouteId)
                .OnDelete(DeleteBehavior.Restrict);

            // SeatDetail
            modelBuilder.Entity<SeatDetail>()
                .HasOne(sd => sd.Trip)
                .WithMany()
                .HasForeignKey(sd => sd.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            //modelBuilder.Entity<Trip>().HasData(
            //    new Trip
            //    {
            //        Id = 1,
            //        From = "Sofia",
            //        To = "Berlin",
            //        Depart = DayOfWeek.Monday,
            //        ReturnDay = DayOfWeek.Tuesday,
            //        DepartureTime = TimeSpan.Parse("01:59:59"),
            //        ReturnTime = TimeSpan.Parse("03:59:59"),
            //        Price = 23.20,
            //        BusId = 1
            //    },
            //    new Trip
            //    {
            //        Id = 2,
            //        From = "Sofia",
            //        To = "Budapesht",
            //        Depart = DayOfWeek.Wednesday,
            //        ReturnDay = DayOfWeek.Thursday,
            //        DepartureTime = TimeSpan.Parse("01:59:59"),
            //        ReturnTime = TimeSpan.Parse("03:59:59"),
            //        Price = 23.20,
            //        BusId = 3
            //    }
            //    );
        }
    }
}
