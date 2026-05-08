namespace AirlineFlightManagement.Services.Contracts
{
    public class ExternalFlightInfo
    {
        public string ExternalApiId { get; set; } = string.Empty;
        public string AirlineName { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public int TotalSeats { get; set; }
        public IEnumerable<ExternalFlightClass> Classes { get; set; } = Array.Empty<ExternalFlightClass>();
    }

    public class ExternalFlightClass
    {
        public string ClassName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int AvailableSeats { get; set; }
    }
}
