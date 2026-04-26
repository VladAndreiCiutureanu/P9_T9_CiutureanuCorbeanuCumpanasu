using AirlineFlightManagement.Models.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AirlineFlightManagement.Services.Interfaces
{
    public interface IAircraftService
    {
        Task<IEnumerable<Aircraft>> GetAllAircraftAsync();
        Task<Aircraft?> GetAircraftByIdAsync(int id);
        Task CreateAircraftAsync(Aircraft aircraft);
        Task UpdateAircraftAsync(Aircraft aircraft);
        Task DeleteAircraftAsync(int id);
    }
}
