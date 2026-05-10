using AirlineFlightManagement.Models.Enums;
using AirlineFlightManagement.Models.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AirlineFlightManagement.DataAccess.Data
{
    // Am modificat clasa de bază în IdentityDbContext<ApplicationUser>
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
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

            // CONFIGURARE ADĂUGATĂ PENTRU COLEGUL A:
            // Maparea relației One-to-One între ApplicationUser și PassengerProfile
            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.PassengerProfile)
                .WithOne(p => p.UserAccount)
                .HasForeignKey<PassengerProfile>(p => p.UserId);

            // RESTUL CONFIGURĂRILOR EXISTENTE (PĂSTRATE EXACT CUM ERAU):
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

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Flight)
                .WithMany(f => f.Reservations)
                .HasForeignKey(r => r.FlightId)
                .OnDelete(DeleteBehavior.Restrict);

            // BR-4: a passenger may not hold more than one active reservation
            // for the exact same flight. The filter excludes Cancelled rows.
            modelBuilder.Entity<Reservation>()
                .HasIndex(r => new { r.PassengerId, r.FlightId })
                .IsUnique()
                .HasFilter("[Status] <> 'Cancelled'");

            // REQ-47: every completed payment must have a unique transaction id.
            modelBuilder.Entity<Payment>()
                .HasIndex(p => p.TransactionId)
                .IsUnique();

            modelBuilder.Entity<SystemConfiguration>()
                .HasIndex(sc => sc.SettingKey)
                .IsUnique();

            modelBuilder.Entity<Flight>()
                .Property(f => f.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            modelBuilder.Entity<Reservation>()
                .Property(r => r.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            modelBuilder.Entity<Payment>()
                .Property(p => p.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            modelBuilder.Entity<Payment>()
                .Property(p => p.PaymentMethod)
                .HasConversion<string>()
                .HasMaxLength(20);
        }
    }
}