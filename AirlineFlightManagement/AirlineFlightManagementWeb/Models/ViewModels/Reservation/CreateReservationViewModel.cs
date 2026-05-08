using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AirlineFlightManagementWeb.Models.ViewModels.Reservation
{
    /// <summary>
    /// Folosit la GET /Reservations/Create?flightId=X (afisare formular)
    /// si la POST /Reservations/Create (input din formular).
    ///
    /// Campurile de Input (FlightId, FlightClassId, SeatId) sunt cele
    /// pe care le trimite browserul. Restul (AirlineName, AvailableClasses,
    /// AvailableSeats) sunt populate de controller pentru afisare.
    /// </summary>
    public class CreateReservationViewModel
    {
        // ─── Input — sunt trimise inapoi la POST ─────────────────────────────
        [Required]
        public int FlightId { get; set; }

        [Required(ErrorMessage = "Selectati o clasa de zbor.")]
        public int FlightClassId { get; set; }

        // Optional — daca user-ul nu alege loc specific, service-ul ia primul disponibil.
        public int? SeatId { get; set; }

        // ─── Display — populate la GET, NU se trimit inapoi la POST ──────────

        public string AirlineName { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }

        // Lista pentru dropdown de clase (Economy / Business / etc.)
        public List<SelectListItem> AvailableClasses { get; set; } = new();

        // Lista pentru radio-uri / dropdown de locuri (cu numar si clasa)
        public List<AvailableSeatViewModel> AvailableSeats { get; set; } = new();
    }

    public class AvailableSeatViewModel
    {
        public int FlightSeatId { get; set; }
        public string SeatNumber { get; set; } = string.Empty;
        public int FlightClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
    }
}
