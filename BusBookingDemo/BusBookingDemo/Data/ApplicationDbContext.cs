using BusBookingDemo.Entity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Mono.TextTemplating;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

namespace BusBookingDemo.Data
{
        public class ApplicationDbContext : DbContext
    {
            public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
            public DbSet<User> User { get; set; }
            public DbSet<Booking> Bookings { get; set; }
            //public DbSet<OrderSeat> OrderSeats { get; set; }
            public DbSet<Trip> Trips { get; set; }
            public DbSet<SeatDetail> SeatDetals { get; set; }
            public DbSet<Bus> Buses { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
            modelBuilder.Entity<Bus>().HasData(
                    new Bus { Id = 1, BusNumber = "234AA", SeatsCount = 24 },
                    new Bus { Id = 2, BusNumber = "211DA", SeatsCount = 34 },
                    new Bus { Id = 3, BusNumber = "232AC", SeatsCount = 20 }
                    );

            modelBuilder.Entity<User>().HasData(
                    new User
                    {
                        Id = 1,
                        FirstName = "tatiana",
                        LastName = "tat",
                        Username = "tati",
                        Password = "1234",
                        Email = "xyz@mail.com",
                        Phone = "12345",
                        Role = "Admin"
                    },
                    new User {
                        Id = 2,
                        FirstName = "anna",
                        LastName = "aniina",
                        Username = "anni",
                        Password = "1234",
                        Email = "xyz1@mail.com",
                        Phone = "12345",
                        Role = "Customer"
                    }
                    );

            // Booking
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Trip)
                .WithMany()
                .HasForeignKey(b => b.TripId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.SeatDetail)
                .WithMany()
                .HasForeignKey(b => b.SeatDetailId)
                .OnDelete(DeleteBehavior.Restrict);

            // Trip
            modelBuilder.Entity<Trip>()
                .HasOne(t => t.Bus)
                .WithMany()
                .HasForeignKey(t => t.BusId)
                .OnDelete(DeleteBehavior.Restrict); // Автобус может быть удален с каскадным удалением поездок

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
