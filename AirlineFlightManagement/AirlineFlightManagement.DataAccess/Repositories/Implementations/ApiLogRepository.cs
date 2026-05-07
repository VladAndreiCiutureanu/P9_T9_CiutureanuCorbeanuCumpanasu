using AirlineFlightManagement.DataAccess.Data;
using AirlineFlightManagement.DataAccess.Repositories.Interfaces;
using AirlineFlightManagement.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace AirlineFlightManagement.DataAccess.Repositories.Implementations
{
    public class ApiLogRepository : Repository<ApiLog>, IApiLogRepository
    {
        public ApiLogRepository(ApplicationDbContext db) : base(db)
        {

        }

        public async Task<IEnumerable<ApiLog>> GetByDateRangeAsync(DateTime from, DateTime to)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(log => log.RequestTimestamp >= from && log.RequestTimestamp <= to)
                .OrderBy(log => log.RequestTimestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<ApiLog>> GetRecentAsync(int count)
        {
            if (count <= 0)
            {
                return Enumerable.Empty<ApiLog>();
            }

            return await _dbSet
                .AsNoTracking()
                .OrderByDescending(log => log.RequestTimestamp)
                .Take(count)
                .ToListAsync();
        }
    }
}
