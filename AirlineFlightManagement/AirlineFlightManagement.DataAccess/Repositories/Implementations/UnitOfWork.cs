using AirlineFlightManagement.Models.Models;
using AirlineFlightManagement.DataAccess.Data;
using AirlineFlightManagement.DataAccess.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AirlineFlightManagement.DataAccess.Repositories.Implementations
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _db;

        public IAircraftRepository AircraftRepository { get; private set; }
        public IRepository<Flight> FlightRepository { get; private set; }

        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            AircraftRepository = new AircraftRepository(_db);
            FlightRepository = new Repository<Flight>(_db);
        }

        public async Task SaveAsync()
        {
            await _db.SaveChangesAsync();
        }

        public void Dispose()
        {
            _db.Dispose();
        }
    }
}
