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
    public class SystemConfigurationRepository : Repository<SystemConfiguration>, ISystemConfigurationRepository
    {
        public SystemConfigurationRepository(ApplicationDbContext db) : base(db)
        {

        }

        public Task<SystemConfiguration?> GetByKeyAsync(string key)
        {
            return _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(sc => sc.SettingKey == key);
        }

        public Task<string?> GetValueAsync(string key)
        {
            return _dbSet
                .AsNoTracking()
                .Where(sc => sc.SettingKey == key)
                .Select(sc => sc.SettingValue)
                .FirstOrDefaultAsync();
        }
    }
}
