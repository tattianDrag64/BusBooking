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
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Schedule> Schedules { get; set; }

        private static readonly Guid SeedRouteId = Guid.Parse("a0000000-0000-0000-0000-000000000001");
        private static readonly Guid SeedScheduleOutboundId = Guid.Parse("d0000000-0000-0000-0000-000000000001");
        private static readonly Guid SeedScheduleReturnId = Guid.Parse("d0000000-0000-0000-0000-000000000002");
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
                        Price = 45.00m
                    });

            modelBuilder.Entity<Schedule>().HasData(
                    new Schedule
                    {
                        Id = SeedScheduleOutboundId,
                        RouteId = SeedRouteId,
                        DayOfWeek = DayOfWeek.Monday,
                        DepartureTime = TimeSpan.Parse("08:00:00"),
                        Duration = TimeSpan.FromHours(10),
                        IsReturnTrip = false
                    },
                    new Schedule
                    {
                        Id = SeedScheduleReturnId,
                        RouteId = SeedRouteId,
                        DayOfWeek = DayOfWeek.Tuesday,
                        DepartureTime = TimeSpan.Parse("18:00:00"),
                        Duration = TimeSpan.FromHours(10),
                        IsReturnTrip = true
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
                        PasswordHash = "$2a$12$DBm2J6TqFjqAWweLi1mxTOT/AyL/XOxQSuvVWkRCbOEHwaaWZHvyy",
                        Email = "xyz@mail.com",
                        Phone = "12345",
                        Role = UserRole.Admin
                    },
                    new User
                    {
                        Id = SeedUser2Id,
                        FirstName = "anna",
                        LastName = "aniina",
                        Username = "anni",
                        PasswordHash = "$2a$12$DBm2J6TqFjqAWweLi1mxTOT/AyL/XOxQSuvVWkRCbOEHwaaWZHvyy",
                        Email = "xyz1@mail.com",
                        Phone = "12345",
                        Role = UserRole.Customer
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

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalPrice)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.CreatedAt)
                .HasColumnType("timestamptz");

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

            modelBuilder.Entity<OrderSeat>()
                .HasIndex(os => os.SeatDetailId)
                .IsUnique();

            // Trip
            modelBuilder.Entity<Trip>()
                .HasOne(t => t.Bus)
                .WithMany()
                .HasForeignKey(t => t.BusId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Trip>()
                .HasOne(t => t.Route)
                .WithMany()
                .HasForeignKey(t => t.RouteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Trip>()
                .Property(t => t.Price)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Trip>()
                .Property(t => t.DepartureDate)
                .HasColumnType("timestamptz");

            modelBuilder.Entity<Trip>()
                .Property(t => t.ArrivalDate)
                .HasColumnType("timestamptz");

            // SeatDetail
            modelBuilder.Entity<SeatDetail>()
                .HasOne(sd => sd.Trip)
                .WithMany()
                .HasForeignKey(sd => sd.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            //routeinfo
            modelBuilder.Entity<RouteInfo>()
                .Property(r => r.Price)
                .HasPrecision(10, 2);

            // Schedule
            modelBuilder.Entity<Schedule>()
                .HasOne(s => s.Route)
                .WithMany(r => r.Schedules)
                .HasForeignKey(s => s.RouteId)
                .OnDelete(DeleteBehavior.Cascade);

            //token
            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany()
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Looked up on every /api/auth/refresh and /api/auth/revoke call — without an
            // index this is a full table scan that gets worse as tokens accumulate.
            modelBuilder.Entity<RefreshToken>()
                .HasIndex(rt => rt.Token)
                .IsUnique();

            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion<string>();

            // AuthController compares via .ToLower(), which Npgsql translates to lower(...) =
            // lower(...) — a plain index on Email/Username wouldn't be used by the query
            // planner for that comparison, so these are expression indexes over lower(...)
            // instead (added via raw SQL in the migration, see AddAuthLookupIndexes).
        }
    }
}
