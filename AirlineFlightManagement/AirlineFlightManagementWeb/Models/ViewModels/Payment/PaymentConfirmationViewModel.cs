using AirlineFlightManagement.Models.Enums;

namespace AirlineFlightManagementWeb.Models.ViewModels.Payment
{
    /// <summary>
    /// Pagina de confirmare dupa o plata reusita.
    /// REQ-50 — notificare clara de succes.
    /// </summary>
    public class PaymentConfirmationViewModel
    {
        public int PaymentId { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }

        // Detalii rezervare confirmata
        public int ReservationId { get; set; }
        public string AirlineName { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public string SeatNumber { get; set; } = string.Empty;
    }
}
