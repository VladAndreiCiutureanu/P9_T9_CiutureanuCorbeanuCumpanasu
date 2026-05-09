using AirlineFlightManagement.Services.Contracts;

namespace AirlineFlightManagementWeb.Models.ViewModels.Report
{
    /// <summary>
    /// Pagina /Reports/Revenue — raport financiar pe interval de date.
    /// REQ-44 — accesibil doar Administratorilor (Staff e exclus de la
    /// rapoartele financiare).
    /// </summary>
    public class RevenueReportViewModel
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        public List<FinancialReportRow> Rows { get; set; } = new();

        // ─── Totaluri agregate ───────────────────────────────────────────────
        public int TotalTransactions => Rows.Sum(r => r.TransactionsCount);

        public decimal GrandTotal => Rows.Sum(r => r.TotalRevenue);

        public decimal AverageDailyRevenue =>
            Rows.Count > 0 ? Math.Round(GrandTotal / Rows.Count, 2) : 0m;
    }
}
