using AirlineFlightManagement.Models.Enums;

namespace AirlineFlightManagementWeb.Models.ViewModels.Reservation
{
    /// <summary>
    /// Pagina de detaliu pentru o rezervare specifica.
    /// Afisat pasagerului (proprietar) si potential staff/admin.
    /// </summary>
    public class ReservationDetailsViewModel
    {
        public int ReservationId { get; set; }

        // Detalii zbor
        public string AirlineName { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }

        // Detalii rezervare
        public string SeatNumber { get; set; } = string.Empty;
        public string PassengerFullName { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public DateTime ReservationTimeStamp { get; set; }
        public ReservationStatus Status { get; set; }

        // Action gates calculate in controller:
        //   CanCancel = Status != Cancelled && DepartureTime > Now + 24h (BR-2)
        //   CanPay    = Status == Pending
        public bool CanCancel { get; set; }
        public bool CanPay { get; set; }
    }
}
