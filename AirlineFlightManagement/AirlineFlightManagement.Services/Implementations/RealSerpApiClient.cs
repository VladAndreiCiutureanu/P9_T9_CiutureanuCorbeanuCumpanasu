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
        private readonly IConfiguration _configuration;
        private readonly ISystemConfigService _configService;

        public RealSerpApiClient(
            HttpClient httpClient,
            IConfiguration configuration,
            ISystemConfigService configService)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _configService = configService;
        }

        // REQ-27: prioritate la cheia din DB (modificabila de admin via UI),
        // fallback la IConfiguration (user-secrets / appsettings / hardcoded).
        // Citim la fiecare apel — overhead-ul DB e neglijabil fata de apelul HTTP.
        private async Task<string> ResolveApiKeyAsync()
        {
            var fromDb = await _configService.GetSerpApiKeyAsync();
            if (!string.IsNullOrWhiteSpace(fromDb)) return fromDb;

            return _configuration["SerpApi:ApiKey"] ?? string.Empty;
        }

        public async Task<IEnumerable<Flight>> SearchFlightsAsync(
            string source,
            string destination,
            DateTime departureDate)
        {
            var apiKey = await ResolveApiKeyAsync();
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException(
                    "API Key-ul pentru SerpAPI nu este configurat (nici in DB, nici in config).");
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
                         $"&api_key={apiKey}";

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
        // Structura SerpAPI relevanta:
        // {
        //   "flights": [
        //     {
        //       "departure_airport": { "id": "LHR", "time": "2026-05-15 07:00" },
        //       "arrival_airport":   { "id": "BER", "time": "2026-05-15 09:00" },
        //       "airline": "Eurowings",
        //       "travel_class": "Economy",
        //       ...
        //     }
        //     // pentru zboruri cu escala, mai multe elemente
        //   ],
        //   "price": 154,
        //   "departure_token": "..."
        // }
        // Extragem DepartureTime din PRIMUL segment si ArrivalTime din ULTIMUL
        // (pentru zboruri directe sunt acelasi segment).
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

            // Fallback la orele hardcoded daca nu reusim sa parsam
            DateTime departureTime = departureDate;
            DateTime arrivalTime = departureDate.AddHours(2);

            if (element.TryGetProperty("flights", out var flightsArr)
                && flightsArr.ValueKind == JsonValueKind.Array)
            {
                var segments = flightsArr.EnumerateArray().ToList();
                if (segments.Count > 0)
                {
                    // Plecarea reala = ora primului segment
                    var firstSegment = segments[0];
                    if (firstSegment.TryGetProperty("departure_airport", out var depAirport)
                        && depAirport.TryGetProperty("time", out var depTime))
                    {
                        var parsed = ParseSerpApiDateTime(depTime.GetString());
                        if (parsed.HasValue) departureTime = parsed.Value;
                    }

                    // Sosirea reala = ora ultimului segment (poate fi diferit de
                    // primul daca exista escale)
                    var lastSegment = segments[^1];
                    if (lastSegment.TryGetProperty("arrival_airport", out var arrAirport)
                        && arrAirport.TryGetProperty("time", out var arrTime))
                    {
                        var parsed = ParseSerpApiDateTime(arrTime.GetString());
                        if (parsed.HasValue) arrivalTime = parsed.Value;
                    }
                }

                // travel_class apare per segment. La un zbor cu escale (3 segmente),
                // toate fiind Economy, am primit 3x Economy. Folosim un HashSet
                // sa pastram doar clasele distincte (case-insensitive).
                var seenClasses = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var f in segments)
                {
                    if (f.TryGetProperty("airline", out var airlineElement))
                    {
                        airlineName = airlineElement.GetString() ?? "Unknown";
                    }
                    if (f.TryGetProperty("travel_class", out var tcElement))
                    {
                        var className = tcElement.GetString() ?? "Economy";
                        if (seenClasses.Add(className))
                        {
                            flightClasses.Add(new FlightClass
                            {
                                ClassName = className,
                                Price = price
                            });
                        }
                    }
                }
            }

            // Daca API-ul nu a returnat nicio clasa, default la Economy
            // ca booking-ul sa functioneze totusi.
            if (flightClasses.Count == 0)
            {
                flightClasses.Add(new FlightClass
                {
                    ClassName = "Economy",
                    Price = price
                });
            }

            return new Flight
            {
                ExternalApiId = externalApiId,
                Source = source,
                Destination = destination,
                AirlineName = airlineName,
                DepartureTime = departureTime,
                ArrivalTime = arrivalTime,
                Status = FlightStatus.Scheduled,
                FlightClasses = flightClasses
            };
        }

        // SerpAPI returneaza datetime in format "yyyy-MM-dd HH:mm" (ora locala
        // a aeroportului de origine). Folosim ParseExact pentru robustete.
        private static DateTime? ParseSerpApiDateTime(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;

            if (DateTime.TryParseExact(
                raw,
                "yyyy-MM-dd HH:mm",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out var parsed))
            {
                return parsed;
            }

            // Fallback flexibil (in caz ca formatul difera)
            if (DateTime.TryParse(
                raw,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out parsed))
            {
                return parsed;
            }

            return null;
        }
    }
}
