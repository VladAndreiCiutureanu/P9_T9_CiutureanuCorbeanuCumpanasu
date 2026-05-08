using System.Threading.Tasks;
using AirlineFlightManagement.Services.Interfaces;

namespace AirlineFlightManagement.Services.Implementations
{
    public class SystemConfigService : ISystemConfigService
    {
        private decimal _currentMarkup = 25.00m; 

        public Task<decimal> GetPlatformMarkupAsync()
        {
            return Task.FromResult(_currentMarkup);
        }

        public Task UpdatePlatformMarkupAsync(decimal newMarkup)
        {
            _currentMarkup = newMarkup;
            return Task.CompletedTask;
        }
    }
}
