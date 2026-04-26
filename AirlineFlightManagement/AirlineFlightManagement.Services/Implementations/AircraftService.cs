using AirlineFlightManagement.DataAccess.Repositories.Interfaces;
using AirlineFlightManagement.Models.Models;
using AirlineFlightManagement.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace AirlineFlightManagement.Services.Implementations
{
    public class AircraftService : IAircraftService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AircraftService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task CreateAircraftAsync(Aircraft aircraft)
        {
            ValidateAircraft(aircraft);
            var existingAircraft = await _unitOfWork.AircraftRepository.GetAsync(a => a.ModelName.ToLower() == aircraft.ModelName.ToLower());
            if (existingAircraft != null)
            {
                throw new InvalidOperationException($"An aircraft with the model name '{aircraft.ModelName}' already exists.");
            }

            await _unitOfWork.AircraftRepository.AddAsync(aircraft);
            await _unitOfWork.SaveAsync();
        }

        public async Task DeleteAircraftAsync(int id)
        {
            var aircraft = await _unitOfWork.AircraftRepository.GetAsync(a => a.AircraftId == id);
            if(aircraft != null)
            {
                _unitOfWork.AircraftRepository.Remove(aircraft);
                await _unitOfWork.SaveAsync();
            }
            else
            {
                throw new KeyNotFoundException($"No aircraft found with ID {id}.");
            }
        }

        public async Task<Aircraft?> GetAircraftByIdAsync(int id)
        {
            return await _unitOfWork.AircraftRepository.GetAsync(a => a.AircraftId == id);
        }

        public async Task<IEnumerable<Aircraft>> GetAllAircraftAsync()
        {
            return await _unitOfWork.AircraftRepository.GetAllAsync();
        }

        public async Task UpdateAircraftAsync(Aircraft aircraft)
        {
            ValidateAircraft(aircraft);
            var existingAircraft = await _unitOfWork.AircraftRepository.GetAsync(
                a => a.ModelName.ToLower() == aircraft.ModelName.ToLower() && a.AircraftId != aircraft.AircraftId
                );
            if( existingAircraft != null)
            {
                throw new InvalidOperationException($"Another aircraft with the model name '{aircraft.ModelName}' already exists.");
            }

            _unitOfWork.AircraftRepository.Update(aircraft);
            await _unitOfWork.SaveAsync();
        }

        private void ValidateAircraft(Aircraft aircraft)
        {
            if(aircraft == null)
                throw new ArgumentNullException(nameof(aircraft), "Aircraft cannot be null.");
            if (string.IsNullOrWhiteSpace(aircraft.ModelName))
                throw new ArgumentException("Model name cannot be empty.");
            if (aircraft.MaxCapacity <= 0)
                throw new ArgumentException("Max capacity must be greater than zero.");
        }
    }
}
