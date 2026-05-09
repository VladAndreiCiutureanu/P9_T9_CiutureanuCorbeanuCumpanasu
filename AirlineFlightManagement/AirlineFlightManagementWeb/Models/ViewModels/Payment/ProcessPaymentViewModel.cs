using System.ComponentModel.DataAnnotations;
using AirlineFlightManagement.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AirlineFlightManagementWeb.Models.ViewModels.Payment
{
    /// <summary>
    /// Folosit la GET /Payments/Process/{reservationId} (afisare formular)
    /// si la POST /Payments/Process (input din formular).
    ///
    /// Suma platita NU e in input. Ea se citeste server-side din DB
    /// (Reservation.TotalPrice) ca sa nu poata fi falsificata din browser.
    /// </summary>
    public class ProcessPaymentViewModel
    {
        // ─── Input ───────────────────────────────────────────────────────────

        [Required]
        public int ReservationId { get; set; }

        [Required(ErrorMessage = "Selectati metoda de plata.")]
        public PaymentMethod PaymentMethod { get; set; }

        // Lista pentru dropdown — populata din enum
        public List<SelectListItem> AvailablePaymentMethods { get; set; } = new();

        // ─── Display only — populate la GET ──────────────────────────────────

        public string AirlineName { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public string SeatNumber { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
    }
}
