using AirlineFlightManagement.DataAccess.Data;
using AirlineFlightManagement.DataAccess.Repositories.Interfaces;
using AirlineFlightManagement.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AirlineFlightManagement.DataAccess.Repositories.Implementations
{
    public class FlightSeatRepository : Repository<FlightSeat>, IFlightSeatRepository
    {

        public FlightSeatRepository(ApplicationDbContext db) : base(db)
        {

        }

        public Task<int> CountAvailableAsync(int flightId)
        {
            return _dbSet
                .AsNoTracking()
                .CountAsync(seat => seat.FlightId == flightId && seat.IsAvailable);
        }

        public Task<int> CountSoldAsync(int flightId)
        {
            return _dbSet
                .AsNoTracking()
                .CountAsync(seat => seat.FlightId == flightId && !seat.IsAvailable);
        }

        public async Task<IEnumerable<FlightSeat>> GetAvailableAsync(int flightId, int? flightClassId = null)
        {
            IQueryable<FlightSeat> query = _dbSet
                .AsNoTracking()
                .Where(seat => seat.FlightId == flightId && seat.IsAvailable);

            if (flightClassId.HasValue)
            {
                query = query.Where(seat => seat.FlightClassId == flightClassId.Value);
            }

            return await query.ToListAsync();
        }
    }
}
