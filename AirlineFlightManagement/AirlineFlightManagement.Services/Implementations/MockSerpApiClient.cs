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

            // ID-uri unice per ruta + data, ca sa nu coliziuneze cu zboruri
            // deja cached in DB pentru alte rute. Fara asta, indexul unique pe
            // ExternalApiId facea ca a doua cautare (alta ruta) sa nu salveze
            // nimic in DB si rezultatele sa apara goale.
            string routeKey = $"{Sanitize(source)}_{Sanitize(destination)}_{departureDate:yyyyMMdd}";

            return new List<Flight>
            {
                new Flight
                {
                    ExternalApiId = $"mock_tarom_{routeKey}",
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
                    ExternalApiId = $"mock_wizz_{routeKey}",
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

        // Inlocuieste caractere care ar putea face urat ExternalApiId-ul:
        // spatii, diacritice, slash-uri. Pastreaza doar litere/cifre/underscore.
        private static string Sanitize(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "X";
            var chars = input
                .Where(c => char.IsLetterOrDigit(c))
                .Select(char.ToUpperInvariant)
                .ToArray();
            return chars.Length > 0 ? new string(chars) : "X";
        }
    }
}
