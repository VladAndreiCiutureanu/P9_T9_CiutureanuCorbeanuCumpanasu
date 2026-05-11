using AirlineFlightManagement.DataAccess.Repositories.Interfaces;
using AirlineFlightManagement.Models.Models;
using AirlineFlightManagement.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AirlineFlightManagement.Services.Implementations
{
    public class PassengerService : IPassengerService
    {
        private readonly IUnitOfWork _uow;

        public PassengerService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<PassengerProfile?> GetByUserIdAsync(string userId)
        {
            return await _uow.PassengerProfileRepository.GetByUserIdAsync(userId);
        }

        public async Task<PassengerProfile?> GetByIdAsync(int passengerId)
        {
            return await _uow.PassengerProfileRepository.GetByIdAsync(passengerId);
        }

        public async Task<IEnumerable<PassengerProfile>> GetAllAsync()
        {
            // Includem proprietatea de navigare "UserAccount" (Eager Loading) pentru a ne asigura că
            // datele de cont (cum ar fi Email și IsActive) sunt disponibile și nu apar ca "N/A" în UI.
            return await _uow.PassengerProfileRepository.GetAllAsync(
                filter: null,
                includeProperties: "UserAccount",
                tracked: false);
        }

        public async Task UpdateProfileAsync(
            string userId,
            string firstName,
            string lastName,
            string phoneNumber,
            string address)
        {
            // Preluăm profilul de pasager asociat acestui cont
            var profile = await _uow.PassengerProfileRepository.GetByUserIdAsync(userId);
            if (profile == null)
            {
                throw new System.Collections.Generic.KeyNotFoundException("Profilul de pasager nu a fost găsit.");
            }

            // Actualizăm atributele de identificare și contact
            profile.FirstName = firstName;
            profile.LastName = lastName;
            profile.PhoneNumber = phoneNumber;
            profile.Address = address;

            // Transmitem entitatea modificată către metoda Update a repository-ului
            _uow.PassengerProfileRepository.Update(profile);
            await _uow.SaveAsync();
        }
    }
}