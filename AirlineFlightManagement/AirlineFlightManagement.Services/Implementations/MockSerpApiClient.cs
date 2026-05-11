using AirlineFlightManagement.Models.Enums;
using AirlineFlightManagement.Models.Models;
using AirlineFlightManagement.Services.Interfaces;

namespace AirlineFlightManagement.Services.Implementations
{
    /// <summary>
    /// Implementare in-memory pentru ISerpApiClient.
    /// Folosita in dev/test cand nu vrem sa consumam din quota SerpAPI.
    /// </summary>
    public class MockSerpApiClient : ISerpApiClient
    {
        public async Task<IEnumerable<Flight>> SearchFlightsAsync(
            string source,
            string destination,
            DateTime departureDate)
        {
            // Mic delay ca sa simulam latenta retea
            await Task.Delay(500);

            return new List<Flight>
            {
                new Flight
                {
                    ExternalApiId = "mock_serp_1",
                    AirlineName = "Tarom",
                    Source = source,
                    Destination = destination,
                    DepartureTime = departureDate.AddHours(8),
                    ArrivalTime = departureDate.AddHours(10),
                    Status = FlightStatus.Scheduled,
                    FlightClasses = new List<FlightClass>
                    {
                        new FlightClass { ClassName = "Economy", Price = 100.00m },
                        new FlightClass { ClassName = "Business", Price = 300.00m }
                    }
                },
                new Flight
                {
                    ExternalApiId = "mock_serp_2",
                    AirlineName = "Wizz Air",
                    Source = source,
                    Destination = destination,
                    DepartureTime = departureDate.AddHours(14),
                    ArrivalTime = departureDate.AddHours(16),
                    Status = FlightStatus.Scheduled,
                    FlightClasses = new List<FlightClass>
                    {
                        new FlightClass { ClassName = "Economy", Price = 45.00m }
                    }
                }
            };
        }

        // Mock-ul nu poate verifica disponibilitate reala — returnam mereu true.
        // In productie, RealSerpApiClient ar interoga API-ul real.
        public Task<bool> VerifyAvailabilityAsync(string externalApiId)
        {
            return Task.FromResult(true);
        }
    }
}
