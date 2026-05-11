using AirlineFlightManagement.Models.Models;

namespace AirlineFlightManagement.Services.Interfaces
{
    /// <summary>
    /// Wrapper peste API-ul extern de zboruri (SerpAPI).
    /// Are 2 implementari: MockSerpApiClient (pentru dev/test) si
    /// RealSerpApiClient (pentru productie).
    /// </summary>
    public interface ISerpApiClient
    {
        // Cauta zboruri pe API extern (REQ-22).
        Task<IEnumerable<Flight>> SearchFlightsAsync(
            string source,
            string destination,
            DateTime departureDate);

        // REQ-25 — verificare disponibilitate inainte de finalizarea rezervarii.
        // Returneaza false daca zborul a fost anulat sau este full.
        // Mock-ul returneaza mereu true (pentru ca nu poate verifica real-time).
        Task<bool> VerifyAvailabilityAsync(string externalApiId);
    }
}
