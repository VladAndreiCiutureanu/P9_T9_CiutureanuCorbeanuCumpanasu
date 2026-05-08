using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AirlineFlightManagement.Models.Models;
using AirlineFlightManagement.Services.Interfaces;

namespace AirlineFlightManagement.Services.Implementations
{
    public class MockSerpApiClient : ISerpApiClient
    {
        public async Task<IEnumerable<Flight>> SearchFlightsAsync(string source, string destination, DateTime departureDate)
        {
            await Task.Delay(500);

            // date false
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
                    Status = "Scheduled",
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
                    Status = "Scheduled",
                    FlightClasses = new List<FlightClass>
                    {
                        new FlightClass { ClassName = "Economy", Price = 45.00m } // Zbor mai ieftin
                    }
                }
            };
        }
    }
}
