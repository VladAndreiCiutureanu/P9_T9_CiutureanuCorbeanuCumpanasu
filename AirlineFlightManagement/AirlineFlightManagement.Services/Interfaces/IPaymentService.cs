using AirlineFlightManagement.Models.Enums;
using AirlineFlightManagement.Models.Models;
using AirlineFlightManagement.Services.Contracts;

namespace AirlineFlightManagement.Services.Interfaces
{
    public interface IPaymentService
    {
        // REQ-42, REQ-43, REQ-45, REQ-47, 5.2 (Transaction Rollback):
        //   - tranzactie completa
        //   - rollback automat la failure
        //   - genereaza TransactionId unic
        //   - la succes: Reservation.Status -> ConfirmedAndPaid
        Task<PaymentResult> ProcessAsync(
            int reservationId,
            PaymentMethod method,
            decimal amount);

        Task<IEnumerable<Payment>> GetByReservationAsync(int reservationId);

        // Istoric plati per pasager (pentru pagina "platile mele")
        Task<IEnumerable<Payment>> GetHistoryAsync(int passengerId);
    }
}
