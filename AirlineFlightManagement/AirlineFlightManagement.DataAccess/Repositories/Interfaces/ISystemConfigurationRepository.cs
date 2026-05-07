using AirlineFlightManagement.Models.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AirlineFlightManagement.DataAccess.Repositories.Interfaces
{
    public interface ISystemConfigurationRepository : IRepository<SystemConfiguration>
    {
        Task<SystemConfiguration?> GetByKeyAsync(string key);

        Task<string?> GetValueAsync(string key);
    }
}
