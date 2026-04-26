using AirlineFlightManagement.DataAccess.Data;
using AirlineFlightManagement.DataAccess.Repositories.Interfaces;
using AirlineFlightManagement.Models.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AirlineFlightManagement.DataAccess.Repositories.Implementations
{
    public class AircraftRepository : Repository<Aircraft>, IAircraftRepository
    {
        private readonly ApplicationDbContext _db;
        public AircraftRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Aircraft obj)
        {
            var objFromDb = _db.Aircrafts.FirstOrDefault(a => a.AircraftId == obj.AircraftId);
            if(objFromDb != null)
            {
                objFromDb.ModelName = obj.ModelName;
                objFromDb.MaxCapacity = obj.MaxCapacity;
            }
        }
    }
}
