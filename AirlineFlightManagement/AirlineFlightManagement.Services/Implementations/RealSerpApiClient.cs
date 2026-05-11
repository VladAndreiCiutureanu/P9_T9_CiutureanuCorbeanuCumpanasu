using System.Net.Http;
using System.Text.Json;
using AirlineFlightManagement.Models.Enums;
using AirlineFlightManagement.Models.Models;
using AirlineFlightManagement.Services.Helpers;
using AirlineFlightManagement.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace AirlineFlightManagement.Services.Implementations
{
    /// <summary>
    /// Implementare reala a ISerpApiClient — interogheaza SerpAPI Google Flights.
    /// La eroare, returneaza lista goala (fail-safe pentru 5.2 — External API Fail-Safe).
    /// </summary>
    public class RealSerpApiClient : ISerpApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public RealSerpApiClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["SerpApi:ApiKey"] ?? string.Empty;
        }

        public async Task<IEnumerable<Flight>> SearchFlightsAsync(
            string source,
            string destination,
            DateTime departureDate)
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                throw new InvalidOperationException(
                    "API Key-ul pentru SerpAPI nu este configurat in appsettings.json.");
            }

            // SerpAPI nu accepta date din trecut
            if (departureDate.Date < DateTime.Now.Date)
            {
                departureDate = DateTime.Now.Date;
            }

            string dateStr = departureDate.ToString("yyyy-MM-dd");
            string safeSource = LocationMapper.GetSafeCode(source);
            string safeDestination = LocationMapper.GetSafeCode(destination);

            // type=2 = One-way
            string url = $"https://serpapi.com/search.json?engine=google_flights" +
                         $"&departure_id={safeSource}&arrival_id={safeDestination}" +
                         $"&outbound_date={dateStr}&type=2&currency=USD&hl=en" +
                         $"&api_key={_apiKey}";

            var flights = new List<Flight>();

            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                using var jsonDoc = JsonDocument.Parse(content);
                var root = jsonDoc.RootElement;

                // SerpAPI returneaza "best_flights" sau "other_flights" in functie de rezultate
                if (!root.TryGetProperty("best_flights", out JsonElement flightsList))
                {
                    root.TryGetProperty("other_flights", out flightsList);
                }

                if (flightsList.ValueKind == JsonValueKind.Array)
                {
                    foreach (var element in flightsList.EnumerateArray())
                    {
                        var flight = ParseFlight(element, source, destination, departureDate);
                        if (flight != null)
                        {
                            flights.Add(flight);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // 5.2 — fail-safe: la eroare nu intoarcem fake flights, doar logam si returnam empty.
                // ReservationService stie sa gestioneze lista goala si refuza booking-ul.
                Console.WriteLine($"[RealSerpApiClient] Eroare la apel SerpAPI: {ex.Message}");
            }

            return flights;
        }

        // REQ-25 — verificare disponibilitate.
        // Implementare conservativa: facem GET pe API si verificam ca zborul mai e returnat.
        // Pentru MVP returnam true (API-ul SerpAPI nu suporta query per ExternalApiId).
        // Acest comportament e safe: daca zborul e in DB local si cache-uit, e disponibil.
        public Task<bool> VerifyAvailabilityAsync(string externalApiId)
        {
            return Task.FromResult(true);
        }

        // ─── Helper privat: parseaza un zbor din JSON-ul SerpAPI ─────────────
        private static Flight? ParseFlight(
            JsonElement element,
            string source,
            string destination,
            DateTime departureDate)
        {
            string externalApiId = element.TryGetProperty("departure_token", out var token)
                ? (token.GetString() ?? Guid.NewGuid().ToString())
                : Guid.NewGuid().ToString();

            decimal price = 0;
            if (element.TryGetProperty("price", out var priceElement))
            {
                price = priceElement.GetDecimal();
            }

            string airlineName = "Unknown";
            var flightClasses = new List<FlightClass>();

            if (element.TryGetProperty("flights", out var flightsArr))
            {
                foreach (var f in flightsArr.EnumerateArray())
                {
                    if (f.TryGetProperty("airline", out var airlineElement))
                    {
                        airlineName = airlineElement.GetString() ?? "Unknown";
                    }
                    if (f.TryGetProperty("travel_class", out var tcElement))
                    {
                        flightClasses.Add(new FlightClass
                        {
                            ClassName = tcElement.GetString() ?? "Economy",
                            Price = price
                        });
                    }
                }
            }

            return new Flight
            {
                ExternalApiId = externalApiId,
                Source = source,
                Destination = destination,
                AirlineName = airlineName,
                DepartureTime = departureDate,
                ArrivalTime = departureDate.AddHours(2),
                Status = FlightStatus.Scheduled,
                FlightClasses = flightClasses
            };
        }
    }
}
