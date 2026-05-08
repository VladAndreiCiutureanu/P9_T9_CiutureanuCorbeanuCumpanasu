using AirlineFlightManagement.Services.Contracts;

namespace AirlineFlightManagement.Services.Interfaces
{
    public interface IReportService
    {
        // REQ-46 — raport ocupare pentru un zbor (% locuri vandute)
        Task<OccupancyReport> GetFlightOccupancyAsync(int flightId);

        // REQ-46 (varianta extinsa) — raport ocupare pentru toate zborurile
        // dintr-un interval de timp
        Task<IEnumerable<OccupancyReport>> GetOccupancyForRangeAsync(
            DateTime from,
            DateTime to);

        // REQ-44 — raport financiar agregat zilnic pentru intervalul cerut
        Task<IEnumerable<FinancialReportRow>> GetRevenueAsync(
            DateTime from,
            DateTime to);
    }
}
