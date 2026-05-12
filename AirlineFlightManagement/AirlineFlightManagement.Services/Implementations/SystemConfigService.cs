using AirlineFlightManagement.DataAccess.Repositories.Interfaces;
using AirlineFlightManagement.Models.Models;
using AirlineFlightManagement.Services.Interfaces;

namespace AirlineFlightManagement.Services.Implementations
{
    /// <summary>
    /// Implementare ISystemConfigService — REQ-27.
    /// Persistenta in tabelul SystemConfigurations via ISystemConfigurationRepository.
    /// </summary>
    public class SystemConfigService : ISystemConfigService
    {
        private readonly IUnitOfWork _uow;

        public SystemConfigService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public Task<string?> GetValueAsync(string key)
        {
            return _uow.SystemConfigurationRepository.GetValueAsync(key);
        }

        public async Task SetValueAsync(string key, string value)
        {
            // Cautam TRACKED pentru a putea modifica si salva
            var existing = await _uow.SystemConfigurationRepository.GetAsync(
                sc => sc.SettingKey == key,
                tracked: true);

            if (existing == null)
            {
                await _uow.SystemConfigurationRepository.AddAsync(new SystemConfiguration
                {
                    SettingKey = key,
                    SettingValue = value,
                    LastUpdated = DateTime.UtcNow
                });
            }
            else
            {
                existing.SettingValue = value;
                existing.LastUpdated = DateTime.UtcNow;
                _uow.SystemConfigurationRepository.Update(existing);
            }

            await _uow.SaveAsync();
        }

        public Task<SystemConfiguration?> GetMetadataAsync(string key)
        {
            return _uow.SystemConfigurationRepository.GetByKeyAsync(key);
        }

        // Helper-e specifice pentru cheia SerpAPI
        public Task<string?> GetSerpApiKeyAsync()
            => GetValueAsync(ISystemConfigService.SerpApiKeyName);

        public Task UpdateSerpApiKeyAsync(string newKey)
            => SetValueAsync(ISystemConfigService.SerpApiKeyName, newKey);
    }
}
