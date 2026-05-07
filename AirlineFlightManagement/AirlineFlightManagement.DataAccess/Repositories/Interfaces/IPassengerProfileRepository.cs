using AirlineFlightManagement.Models.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AirlineFlightManagement.DataAccess.Repositories.Interfaces
{
    public interface IPassengerProfileRepository : IRepository<PassengerProfile>
    {
        Task<PassengerProfile?> GetByUserIdAsync(string userId);

        Task<PassengerProfile?> GetWithReservationsAsync(int passengerId);
    }
}
