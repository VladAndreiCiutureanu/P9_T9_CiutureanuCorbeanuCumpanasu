using AirlineFlightManagement.Models.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AirlineFlightManagement.DataAccess.Repositories.Interfaces
{
    public interface IFlightRepository : IRepository<Flight>
    {
        Task<IEnumerable<Flight>> SearchAsync(string source, string destination, DateTime date);
        Task<Flight?> GetByExternalApiIdAsync(string externalApiId);

        Task<Flight?> GetWithDetailsAsync(int flightId);

        Task<bool> HasActiveReservationsAsync(int flightId);
    }
}
