using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AirlineFlightManagement.Models.Models;
using AirlineFlightManagement.Services.Interfaces;
using AirlineFlightManagement.DataAccess.Repositories.Interfaces;

namespace AirlineFlightManagement.Services.Implementations
{
    public class FlightService : IFlightService
    {
        private readonly ISerpApiClient _serpApiClient;
        private readonly ISystemConfigService _configService;
        private readonly IUnitOfWork _unitOfWork;

        public FlightService(ISerpApiClient serpApiClient, ISystemConfigService configService, IUnitOfWork unitOfWork)
        {
            _serpApiClient = serpApiClient;
            _configService = configService;
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Flight>> GetAvailableFlightsAsync(string source, string destination, DateTime departureDate)
        {
            // First check the database
            var dbFlights = await _unitOfWork.FlightRepository.GetAllAsync(f => f.Source == source && f.Destination == destination && f.DepartureTime.Date == departureDate.Date, includeProperties: "FlightClasses");

            var flightsList = dbFlights.ToList();
            if (flightsList.Any())
            {
                // Flights found in database, apply markup and return
                return await ApplyMarkupAsync(flightsList);
            }

            // Not found in database, call the API
            var rawFlights = await _serpApiClient.SearchFlightsAsync(source, destination, departureDate);

            // Fetch the default aircraft, or create it if no aircrafts exist in the database yet
            var aircrafts = await _unitOfWork.AircraftRepository.GetAllAsync();
            var targetAircraft = aircrafts.FirstOrDefault();

            if (targetAircraft == null)
            {
                targetAircraft = new Aircraft
                {
                    ModelName = "External API Default Aircraft",
                    MaxCapacity = 200 // Some default capacity
                };
                await _unitOfWork.AircraftRepository.AddAsync(targetAircraft);
                await _unitOfWork.SaveAsync(); 
            }

            // Verificăm și salvăm zborurile în baza de date dacă nu există deja
            var existingFlights = await _unitOfWork.FlightRepository.GetAllAsync();
            foreach(var flight in rawFlights)
            {
                // Ignorăm zborurile false de eroare (care au id-ul de EROARE API) la salvare
                if (flight.Source != "EROARE" && !existingFlights.Any(f => f.ExternalApiId == flight.ExternalApiId))
                {
                    flight.AircraftId = targetAircraft.AircraftId;
                    await _unitOfWork.FlightRepository.AddAsync(flight);
                }
            }
            // Save inside a try block just in case
            try
            {
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                // Log or swallow if the db fails again just to avoid crashing the view
                Console.WriteLine("Could not save to db: " + ex.Message);
            }

            return await ApplyMarkupAsync(rawFlights.ToList());
        }

        private async Task<IEnumerable<Flight>> ApplyMarkupAsync(List<Flight> flights)
        {
            var markup = await _configService.GetPlatformMarkupAsync();

            var updatedFlights = flights.ToList();
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
