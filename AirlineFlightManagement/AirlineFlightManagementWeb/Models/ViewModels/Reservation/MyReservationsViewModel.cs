using AirlineFlightManagement.Models.Enums;

namespace AirlineFlightManagementWeb.Models.ViewModels.Reservation
{
    /// <summary>
    /// Container pentru pagina /Reservations (Index).
    /// REQ-36 — istoric rezervari ale pasagerului curent.
    /// </summary>
    public class MyReservationsViewModel
    {
        public List<ReservationListItemViewModel> Reservations { get; set; } = new();
    }

    /// <summary>
    /// Un rand din lista de rezervari afisata pe Index.
    /// </summary>
    public class ReservationListItemViewModel
    {
        public int ReservationId { get; set; }
        public string AirlineName { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public string SeatNumber { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public ReservationStatus Status { get; set; }

        // Calculat in controller pe baza BR-2 si Status:
        //   - Status != Cancelled
        //   - DepartureTime > Now + 24h
        public bool CanCancel { get; set; }

        // True daca status == Pending (utilizator poate plati)
        public bool CanPay { get; set; }
    }
}
