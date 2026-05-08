using System.Threading.Tasks;

namespace AirlineFlightManagement.Services.Interfaces
{
    public interface ISystemConfigService
    {
        Task<decimal> GetPlatformMarkupAsync();
        Task UpdatePlatformMarkupAsync(decimal newMarkup);
    }
}
