using AirlineFlightManagement.Models.Models;

namespace AirlineFlightManagement.Services.Interfaces
{
    /// <summary>
    /// REQ-27 — gestiunea credentialelor si setarilor de sistem.
    /// Admin poate configura/actualiza valori (ex: cheia SerpAPI) via UI.
    /// Stocare persistenta in tabelul SystemConfigurations.
    /// </summary>
    public interface ISystemConfigService
    {
        // Get/Set generic pe orice cheie.
        Task<string?> GetValueAsync(string key);
        Task SetValueAsync(string key, string value);

        // Metadata completa (LastUpdated etc.) pentru afisare in UI admin.
        Task<SystemConfiguration?> GetMetadataAsync(string key);

        // Helper-e specifice pentru cheia SerpAPI — folosite peste tot in cod
        // ca sa nu pasam constante "SerpApi:ApiKey" peste tot.
        Task<string?> GetSerpApiKeyAsync();
        Task UpdateSerpApiKeyAsync(string newKey);

        // Numele cheii in DB pentru SerpAPI — expus public ca alte servicii
        // sa-l poata folosi (ex: seeding la startup).
        const string SerpApiKeyName = "SerpApi:ApiKey";
    }
}
