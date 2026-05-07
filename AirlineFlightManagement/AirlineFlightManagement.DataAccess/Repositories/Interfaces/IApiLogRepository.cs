using AirlineFlightManagement.Models.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AirlineFlightManagement.DataAccess.Repositories.Interfaces
{
    public interface IApiLogRepository : IRepository<ApiLog>
    {
        Task<IEnumerable<ApiLog>> GetRecentAsync(int count);
        Task<IEnumerable<ApiLog>> GetByDateRangeAsync(DateTime from, DateTime to);
    }
}
