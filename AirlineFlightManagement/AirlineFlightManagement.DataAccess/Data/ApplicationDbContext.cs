using AirlineFlightManagement.Models.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AirlineFlightManagement.DataAccess.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
    {
        public DbSet<PassengerProfile> PassengerProfiles { get; set; }
        public DbSet<Aircraft> Aircrafts { get; set; }
        public DbSet<Flight> Flights { get; set; }
        public DbSet<FlightClass> FlightClasses { get; set; }
        public DbSet<FlightSeat> FlightSeats { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<SystemConfiguration> SystemConfigurations { get; set; }
        public DbSet<ApiLog> ApiLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<FlightSeat>()
                .HasOne(fs => fs.Flight)
                .WithMany(f => f.FlightSeats)
                .HasForeignKey(fs => fs.FlightId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FlightSeat>()
                .HasOne(fs => fs.FlightClass)
                .WithMany(fc => fc.FlightSeats)
                .HasForeignKey(fs => fs.FlightClassId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
