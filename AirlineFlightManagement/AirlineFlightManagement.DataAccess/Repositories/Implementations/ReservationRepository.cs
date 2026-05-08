using AirlineFlightManagement.DataAccess.Data;
using AirlineFlightManagement.DataAccess.Repositories.Interfaces;
using AirlineFlightManagement.Models.Enums;
using AirlineFlightManagement.Models.Models;
using Microsoft.EntityFrameworkCore;

namespace AirlineFlightManagement.DataAccess.Repositories.Implementations
{
    public class ReservationRepository : Repository<Reservation>, IReservationRepository
    {
        public ReservationRepository(ApplicationDbContext db) : base(db)
        {
        }

        public Task<int> CountConfirmedForFlightAsync(int flightId)
        {
            return _dbSet.AsNoTracking()
                .CountAsync(r => r.FlightId == flightId && r.Status == ReservationStatus.ConfirmedAndPaid);
        }

        public async Task<IEnumerable<Reservation>> GetByPassengerAsync(int passengerId)
        {
            return await _dbSet.AsNoTracking().Where(r => r.PassengerId == passengerId)
                .Include(r => r.Flight)
                .Include(r => r.Seat)
                .ToListAsync();
        }

        public async Task<IEnumerable<Reservation>> GetManifestAsync(int flightId)
        {
            return await _dbSet.AsNoTracking()
                .Where(r => r.FlightId == flightId && r.Status == ReservationStatus.ConfirmedAndPaid)
                .Include(r => r.Passenger)
                .Include(r => r.Seat)
                .ToListAsync();
        }

        public Task<Reservation?> GetWithDetailsAsync(int reservationId)
        {
            return _dbSet.AsNoTracking()
                .Include(r => r.Flight)
                .Include(r => r.Passenger)
                .Include(r => r.Seat)
                .FirstOrDefaultAsync(r => r.ReservationId == reservationId);
        }

        public Task<bool> HasActiveReservationAsync(int passengerId, int flightId)
        {
            return _dbSet.AsNoTracking()
                .AnyAsync(r => r.PassengerId == passengerId
                            && r.FlightId == flightId
                            && r.Status != ReservationStatus.Cancelled);
        }
    }
}
