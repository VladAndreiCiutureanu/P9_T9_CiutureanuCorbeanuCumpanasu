using AirlineFlightManagement.Models.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AirlineFlightManagement.DataAccess.Repositories.Interfaces
{
    public interface IAircraftRepository : IRepository<Aircraft>
    {
        void Update(Aircraft obj);
    }
}
