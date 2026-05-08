using AirlineFlightManagement.DataAccess.Data;
using AirlineFlightManagement.DataAccess.Repositories.Interfaces;
using AirlineFlightManagement.Models.Models;
using Microsoft.EntityFrameworkCore;

namespace AirlineFlightManagement.DataAccess.Repositories.Implementations
{
    public class PassengerProfileRepository : Repository<PassengerProfile>, IPassengerProfileRepository
    {
        public PassengerProfileRepository(ApplicationDbContext db) : base(db)
        {
        }

        public Task<PassengerProfile?> GetByUserIdAsync(string userId)
        {
            return _dbSet.AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == userId);
        }

        public Task<PassengerProfile?> GetWithReservationsAsync(int passengerId)
        {
            return _dbSet.AsNoTracking()
                .Include(p => p.Reservations)
                .FirstOrDefaultAsync(p => p.PassengerId == passengerId);
        }
    }
}
