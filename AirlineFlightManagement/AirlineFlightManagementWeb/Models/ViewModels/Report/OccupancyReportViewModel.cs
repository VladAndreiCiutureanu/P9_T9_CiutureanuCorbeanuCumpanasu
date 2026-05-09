using AirlineFlightManagement.Services.Contracts;

namespace AirlineFlightManagementWeb.Models.ViewModels.Report
{
    /// <summary>
    /// Pagina /Reports/Occupancy — raport ocupare pe interval de date.
    /// REQ-46.
    ///
    /// Refolosim OccupancyReport din Services.Contracts ca element de lista
    /// (nu cream alt VM duplicat) — contractul e curat, fara dependinte UI.
    /// </summary>
    public class OccupancyReportViewModel
    {
        // ─── Filtre (afisate ca form, trimise inapoi prin query string) ─────
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        // ─── Date ────────────────────────────────────────────────────────────
        public List<OccupancyReport> Reports { get; set; } = new();

        // ─── Statistici agregate (calculate in controller) ───────────────────
        public int TotalFlights => Reports.Count;

        public int TotalSold => Reports.Sum(r => r.SoldSeats);

        public int TotalCapacity => Reports.Sum(r => r.TotalSeats);

        public decimal AverageOccupancy =>
            TotalCapacity > 0
                ? Math.Round((decimal)TotalSold / TotalCapacity * 100m, 2)
                : 0m;
    }
}
