using AirlineFlightManagement.Models.Enums;

namespace AirlineFlightManagementWeb.Models.ViewModels.Payment
{
    /// <summary>
    /// Container pentru pagina /Payments/History.
    /// Istoric complet plati pentru pasagerul curent.
    /// </summary>
    public class PaymentHistoryViewModel
    {
        public List<PaymentHistoryItemViewModel> Payments { get; set; } = new();
    }

    public class PaymentHistoryItemViewModel
    {
        public int PaymentId { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentStatus Status { get; set; }

        public int ReservationId { get; set; }

        // String compus afisat in lista, ex: "OTP -> MUC, 15 dec 2026"
        public string FlightInfo { get; set; } = string.Empty;
    }
}
