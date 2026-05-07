using AirlineFlightManagement.Models.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AirlineFlightManagement.DataAccess.Repositories.Interfaces
{
    public interface IFlightClassRepository : IRepository<FlightClass>
    {
        Task<IEnumerable<FlightClass>> GetByFlightAsync(int flightId);
    }
}
