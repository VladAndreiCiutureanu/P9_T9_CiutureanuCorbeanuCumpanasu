using AirlineFlightManagement.Services.Interfaces;

namespace AirlineFlightManagement.Services.Implementations
{
    /// <summary>
    /// Implementare in-memory pentru markup-ul aplicat peste preturile zborurilor.
    /// In productie, valoarea ar trebui persistata in DB (via ISystemConfigService)
    /// — pentru MVP, e suficienta varianta in-memory.
    /// </summary>
    public class MarkupService : IMarkupService
    {
        // Markup-ul default in unitati monetare (USD pentru SerpAPI).
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
