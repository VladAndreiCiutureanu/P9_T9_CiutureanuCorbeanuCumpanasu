using AirlineFlightManagement.Models.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AirlineFlightManagement.DataAccess.Repositories.Interfaces
{
    public interface IReservationRepository : IRepository<Reservation>
    {
        Task<IEnumerable<Reservation>> GetByPassengerAsync(int passengerId);
        Task<IEnumerable<Reservation>> GetManifestAsync(int flightId);
        Task<bool> HasActiveReservationAsync(int passengerId, int flightId);
        Task<int> CountConfirmedForFlightAsync(int flightId);
        Task<Reservation?> GetWithDetailsAsync(int reservationId);
    }
}
