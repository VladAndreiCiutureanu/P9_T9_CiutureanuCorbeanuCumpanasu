using AirlineFlightManagement.Services.Contracts;

namespace AirlineFlightManagement.Services.Interfaces
{
    public interface IAuthService
    {
        // Creeaza ApplicationUser + PassengerProfile in tranzactie.
        // REQ-5, REQ-6, REQ-11, REQ-12, REQ-19
        Task<AuthResult> RegisterAsync(
            string email,
            string password,
            string firstName,
            string lastName,
            string phoneNumber,
            string address);

        // REQ-3 — autentifica si seteaza cookie-ul de sesiune
        Task<AuthResult> LoginAsync(string email, string password, bool rememberMe);

        Task LogoutAsync();

        // REQ-7, BR-3 — Admin atribuie roluri (Administrator, Staff, Customer)
        Task<AuthResult> AssignRoleAsync(string userId, string roleName);

        // REQ-18 — dezactivare logica (IsActive = false), nu sterge fizic
        Task<AuthResult> DeactivateUserAsync(string userId);
    }
}
