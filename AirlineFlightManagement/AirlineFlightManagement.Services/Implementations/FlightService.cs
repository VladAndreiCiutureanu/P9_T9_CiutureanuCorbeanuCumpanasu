using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AirlineFlightManagement.Models.Models;
using AirlineFlightManagement.Services.Interfaces;

namespace AirlineFlightManagement.Services.Implementations
{
    public class FlightService : IFlightService
    {
        private readonly ISerpApiClient _serpApiClient;
        private readonly ISystemConfigService _configService;

        public FlightService(ISerpApiClient serpApiClient, ISystemConfigService configService)
        {
            _serpApiClient = serpApiClient;
            _configService = configService;
        }

        public async Task<IEnumerable<Flight>> GetAvailableFlightsAsync(string source, string destination, DateTime departureDate)
        {
            // luam din mock
            var rawFlights = await _serpApiClient.SearchFlightsAsync(source, destination, departureDate);

            // cerem adaosul
            var markup = await _configService.GetPlatformMarkupAsync();

            // modificam prețul din clasele de zbor conform adaosului
            var updatedFlights = rawFlights.ToList();
            foreach(var flight in updatedFlights)
            {
                if (flight.FlightClasses != null)
                {
                    foreach(var flightClass in flight.FlightClasses)
                    {
                        flightClass.Price += markup;
                    }
                }
            }
            return updatedFlights;
        }
    }
}
