using AirlineFlightManagement.Models.Models;

namespace AirlineFlightManagement.Services.Interfaces
{
    public interface IPassengerService
    {
        Task<PassengerProfile?> GetByUserIdAsync(string userId);

        Task<PassengerProfile?> GetByIdAsync(int passengerId);

        // REQ-17 — admin / staff vede lista
        Task<IEnumerable<PassengerProfile>> GetAllAsync();

        // REQ-15 — utilizatorul autentificat isi modifica datele
        Task UpdateProfileAsync(
            string userId,
            string firstName,
            string lastName,
            string phoneNumber,
            string address);
    }
}
