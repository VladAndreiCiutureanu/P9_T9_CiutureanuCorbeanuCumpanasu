using AirlineFlightManagement.Models.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AirlineFlightManagement.DataAccess.Repositories.Interfaces
{
    public interface IFlightSeatRepository : IRepository<FlightSeat>
    {
        Task<IEnumerable<FlightSeat>> GetAvailableAsync(int flightId, int? flightClassId = null);
        Task<int> CountAvailableAsync(int flightId);
        Task<int> CountSoldAsync(int flightId);
    }
}
