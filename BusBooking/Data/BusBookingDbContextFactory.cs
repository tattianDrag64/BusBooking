using BusBooking.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class BusBookingDbContextFactory : IDesignTimeDbContextFactory<BusBookingDbContext>
{
    public BusBookingDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<BusBookingDbContext>();
        optionsBuilder.UseSqlServer(@"Server=localhost;Database=BusBookingSystemDB;Trusted_Connection=True;TrustServerCertificate=True;");

        return new BusBookingDbContext(optionsBuilder.Options);
    }
}
