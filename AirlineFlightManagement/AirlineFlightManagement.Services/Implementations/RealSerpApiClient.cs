using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using AirlineFlightManagement.Models.Models;
using AirlineFlightManagement.Services.Interfaces;
using AirlineFlightManagement.Services.Helpers;

namespace AirlineFlightManagement.Services.Implementations
{
    public class RealSerpApiClient : ISerpApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public RealSerpApiClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["SerpApi:ApiKey"] ?? string.Empty;
        }

        public async Task<IEnumerable<Flight>> SearchFlightsAsync(string source, string destination, DateTime departureDate)
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                throw new InvalidOperationException("API Key-ul pentru SerpAPI nu este configurat în appsettings.json.");
            }

            var flights = new List<Flight>();

            if (departureDate.Date < DateTime.Now.Date)
            {
                departureDate = DateTime.Now.Date;
            }

            // format the date
            string dateStr = departureDate.ToString("yyyy-MM-dd");

            string safeSource = LocationMapper.GetSafeCode(source);
            string safeDestination = LocationMapper.GetSafeCode(destination);

            // 'type=2' (One-way) 
            string url = $"https://serpapi.com/search.json?engine=google_flights&departure_id={safeSource}&arrival_id={safeDestination}&outbound_date={dateStr}&type=2&currency=USD&hl=en&api_key={_apiKey}";

            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();

                // parse the json
                using var jsonDoc = JsonDocument.Parse(content);
                var root = jsonDoc.RootElement;

  
                if (!root.TryGetProperty("best_flights", out JsonElement flightsList))
                {
                    root.TryGetProperty("other_flights", out flightsList);
                }

                if (flightsList.ValueKind == JsonValueKind.Array)
                {
                    foreach (var element in flightsList.EnumerateArray())
                    {
                        var flight = new Flight
                        {
                            ExternalApiId = element.TryGetProperty("departure_token", out var token) ? token.GetString() : Guid.NewGuid().ToString(),
                            Source = source,
                            Destination = destination,
                            DepartureTime = departureDate, 
                            ArrivalTime = departureDate.AddHours(2), 
                            Status = "Scheduled",
                            FlightClasses = new List<FlightClass>()
                        };

                        // Parse price
                        decimal price = 0;
                        if (element.TryGetProperty("price", out var priceElement))
                        {
                            price = priceElement.GetDecimal();
                        }

                        // Parse flights array to get airline etc.
                        if (element.TryGetProperty("flights", out var flightsArr))
                        {
                            foreach (var f in flightsArr.EnumerateArray())
                            {
                                if (f.TryGetProperty("airline", out var airlineElement))
                                {
                                    flight.AirlineName = airlineElement.GetString();
                                }
                                if (f.TryGetProperty("travel_class", out var tcElement))
                                {
                                    var flightClass = new FlightClass
                                    {
                                        ClassName = tcElement.GetString(),
                                        Price = price
                                    };
                                    flight.FlightClasses.Add(flightClass);
                                }
                            }
                        }

                        flights.Add(flight);
                    }
                }
            }
            catch (Exception ex)
            {
                
                var errorFlight = new Flight
                {
                    ExternalApiId = Guid.NewGuid().ToString(),
                    Source = "EROARE",
                    Destination = "API",
                    AirlineName = ex.Message.Length > 50 ? ex.Message.Substring(0, 50) + "..." : ex.Message, 
                    Status = "Eroare Parsare: " + ex.StackTrace?.Substring(0, Math.Min(100, ex.StackTrace?.Length ?? 0)),
                    DepartureTime = DateTime.Now,
                    ArrivalTime = DateTime.Now,
                    FlightClasses = new List<FlightClass>
                    {
                        new FlightClass { ClassName = "Error", Price = 0 }
                    }
                };
                flights.Add(errorFlight);
            }

            return flights;
        }
    }
}
