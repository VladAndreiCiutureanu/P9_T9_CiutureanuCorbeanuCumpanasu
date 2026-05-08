namespace AirlineFlightManagement.Services.Interfaces
{
    public interface ISystemConfigService
    {
        // REQ-27 — getter generic pentru orice setare
        Task<string?> GetValueAsync(string key);

        // REQ-27 — admin updateaza valoarea (creeaza intrarea daca nu exista)
        Task SetValueAsync(string key, string value);

        // Helper-e specifice pentru cea mai folosita setare (credentialele Amadeus)
        Task<string?> GetAmadeusApiKeyAsync();
        Task UpdateAmadeusApiKeyAsync(string newKey);
    }
}
