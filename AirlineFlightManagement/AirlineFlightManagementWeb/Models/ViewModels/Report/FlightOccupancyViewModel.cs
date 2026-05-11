using AirlineFlightManagement.Services.Contracts;

namespace AirlineFlightManagementWeb.Models.ViewModels.Report
{
    /// <summary>
    /// Pagina /Reports/FlightOccupancy/{flightId} — raport pentru un singur zbor.
    /// </summary>
    public class FlightOccupancyViewModel
    {
        public OccupancyReport Report { get; set; } = new();
    }
}
