namespace AirlineFlightManagement.Services.Contracts
{
    public class FinancialReportRow
    {
        public DateTime Date { get; set; }
        public int TransactionsCount { get; set; }
        public decimal TotalRevenue { get; set; }

        public decimal AveragePerTransaction =>
            TransactionsCount > 0 ? Math.Round(TotalRevenue / TransactionsCount, 2) : 0m;
    }
}
