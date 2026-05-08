using AirlineFlightManagement.Models.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AirlineFlightManagement.DataAccess.Repositories.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IAircraftRepository AircraftRepository { get; }
        IRepository<Flight> FlightRepository { get; }
        Task SaveAsync();
    }
}
