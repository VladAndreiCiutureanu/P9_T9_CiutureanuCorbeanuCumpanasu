using AirlineFlightManagement.DataAccess.Data;
using AirlineFlightManagement.DataAccess.Repositories.Interfaces;
using AirlineFlightManagement.Models.Enums;
using AirlineFlightManagement.Models.Models;
using Microsoft.EntityFrameworkCore;

namespace AirlineFlightManagement.DataAccess.Repositories.Implementations
{
    public class FlightRepository : Repository<Flight>, IFlightRepository
    {
        public FlightRepository(ApplicationDbContext db) : base(db)
        {

        }

        public async Task<Flight?> GetByExternalApiIdAsync(string externalApiId)
        {
            return await _dbSet.AsNoTracking().FirstOrDefaultAsync(f => f.ExternalApiId == externalApiId);
        }

        public Task<Flight?> GetWithDetailsAsync(int flightId)
        {
            return _dbSet.AsNoTracking()
                .Include(f => f.FlightClasses)
                .Include(f => f.FlightSeats)
                .FirstOrDefaultAsync(f => f.FlightId == flightId);
        }

        public Task<bool> HasActiveReservationsAsync(int flightId)
        {
            return _db.Set<Reservation>().AsNoTracking()
                .AnyAsync(r => r.FlightId == flightId && r.Status != ReservationStatus.Cancelled);
        }

        public async Task<IEnumerable<Flight>> SearchAsync(string source, string destination, DateTime date)
        {
           return await _dbSet.AsNoTracking()
                .Where(f => f.Source == source && f.Destination == destination && f.DepartureTime.Date == date.Date)
                .ToListAsync();
        }
    }
}
