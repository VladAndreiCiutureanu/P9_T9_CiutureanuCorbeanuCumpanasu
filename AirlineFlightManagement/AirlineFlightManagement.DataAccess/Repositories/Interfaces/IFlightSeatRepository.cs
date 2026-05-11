using AirlineFlightManagement.Models.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AirlineFlightManagement.DataAccess.Repositories.Interfaces
{
    public interface IFlightSeatRepository : IRepository<FlightSeat>
    {
        // tracked = true cand apelantul vrea sa modifice locul gasit
        // (ex: ReservationService il marcheaza IsAvailable = false).
        Task<IEnumerable<FlightSeat>> GetAvailableAsync(
            int flightId,
            int? flightClassId = null,
            bool tracked = false);
        Task<int> CountAvailableAsync(int flightId);
        Task<int> CountSoldAsync(int flightId);
    }
}
