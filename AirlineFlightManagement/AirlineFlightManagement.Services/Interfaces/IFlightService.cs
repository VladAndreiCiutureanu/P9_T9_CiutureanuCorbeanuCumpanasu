using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AirlineFlightManagement.Models.Models;

namespace AirlineFlightManagement.Services.Interfaces
{
    public interface IFlightService
    {
        Task<IEnumerable<Flight>> GetAvailableFlightsAsync(string source, string destination, DateTime departureDate);
    }
}
