namespace AirlineFlightManagement.Services.Contracts
{
    public class OccupancyReport
    {
        public int FlightId { get; set; }
        public string AirlineName { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public int TotalSeats { get; set; }
        public int SoldSeats { get; set; }

        public decimal OccupancyPercentage =>
            TotalSeats > 0 ? Math.Round((decimal)SoldSeats / TotalSeats * 100m, 2) : 0m;
    }
}
