using AirlineFlightManagement.Services.Contracts;

namespace AirlineFlightManagement.Services.Interfaces
{
    public interface IAmadeusClient
    {
        // REQ-20, REQ-22 — cauta zboruri pe API extern
        Task<IEnumerable<ExternalFlightInfo>> SearchAsync(
            string source,
            string destination,
            DateTime date);

        // Returneaza un zbor specific dupa ExternalApiId.
        // Folosit cand admin importa un zbor in DB pentru modificare.
        Task<ExternalFlightInfo?> GetByExternalIdAsync(string externalApiId);

        // REQ-25 — verificare disponibilitate inainte de finalizarea rezervarii.
        // Returneaza false daca zborul a fost anulat sau este full.
        Task<bool> VerifyAvailabilityAsync(string externalApiId);
    }
}
