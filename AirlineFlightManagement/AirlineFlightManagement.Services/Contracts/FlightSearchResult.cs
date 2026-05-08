namespace AirlineFlightManagement.Services.Contracts
{
    public class FlightSearchResult
    {
        public string ExternalApiId { get; set; } = string.Empty;

        // null cand zborul vine doar din API si nu este cache-at local inca
        public int? LocalFlightId { get; set; }

        public string AirlineName { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public int AvailableSeats { get; set; }

        public IEnumerable<FlightClassInfo> Classes { get; set; } = Array.Empty<FlightClassInfo>();

        // True daca rezultatul vine din DB local (pret modificat / cache la booking).
        // Folosit pentru badge-ul "Promotie" in UI.
        public bool IsFromLocalDb { get; set; }
    }

    public class FlightClassInfo
    {
        public int? FlightClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int AvailableSeats { get; set; }
    }
}
