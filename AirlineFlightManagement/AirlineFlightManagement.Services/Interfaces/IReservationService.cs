using AirlineFlightManagement.Models.Models;

namespace AirlineFlightManagement.Services.Interfaces
{
    public interface IReservationService
    {
        // Creeaza rezervarea intr-o tranzactie:
        //   - REQ-25 verifica disponibilitate prin API
        //   - BR-1  verifica capacitatea zborului
        //   - BR-4  verifica unicitate (passenger + flight, status != Cancelled)
        //   - REQ-32 marcheaza locul ca ocupat
        //   - REQ-38 calculeaza pretul total din FlightClass
        //   - status initial = Pending
        Task<Reservation> CreateAsync(
            int passengerId,
            int flightId,
            int flightClassId,
            int? seatId = null);

        // REQ-40 — anulare cu BR-2 (minim 24h inainte de plecare)
        // REQ-41 — elibereaza locul
        Task CancelAsync(int reservationId, int passengerId);

        // REQ-36 — istoric rezervari pasager
        Task<IEnumerable<Reservation>> GetHistoryAsync(int passengerId);

        // REQ-35 — manifest pasageri pentru un zbor (Staff/Admin)
        Task<IEnumerable<Reservation>> GetManifestAsync(int flightId);

        Task<Reservation?> GetWithDetailsAsync(int reservationId);
    }
}
