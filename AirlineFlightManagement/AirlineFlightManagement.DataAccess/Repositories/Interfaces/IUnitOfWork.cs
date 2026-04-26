using System;
using System.Collections.Generic;
using System.Text;

namespace AirlineFlightManagement.DataAccess.Repositories.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IAircraftRepository AircraftRepository { get; }
        Task SaveAsync();

    }
}
