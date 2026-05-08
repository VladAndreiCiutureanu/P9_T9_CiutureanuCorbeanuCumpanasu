namespace AirlineFlightManagementWeb.Models.ViewModels.Reservation
{
    /// <summary>
    /// REQ-35 — manifest pasageri pentru un zbor.
    /// Acces restrictionat: doar Staff si Administrator.
    /// </summary>
    public class ManifestViewModel
    {
        public int FlightId { get; set; }
        public string AirlineName { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }

        public List<ManifestRowViewModel> Passengers { get; set; } = new();
    }

    public class ManifestRowViewModel
    {
        public string PassengerFullName { get; set; } = string.Empty;
        public string SeatNumber { get; set; } = string.Empty;
        public DateTime ReservationTimeStamp { get; set; }
    }
}
