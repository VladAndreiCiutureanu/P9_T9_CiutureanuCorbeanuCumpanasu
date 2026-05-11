namespace AirlineFlightManagement.Services.Interfaces
{
    /// <summary>
    /// Serviciu pentru gestionarea markup-ului aplicat peste preturile
    /// returnate de API-ul extern (SerpAPI). Markup-ul reprezinta comisionul
    /// platformei adunat la pretul original al zborului.
    ///
    /// Conceptual separat de ISystemConfigService (care gestioneaza
    /// chei API si alte setari generale).
    /// </summary>
    public interface IMarkupService
    {
        // Returneaza markup-ul curent (in unitati monetare absolute, nu procent).
        Task<decimal> GetPlatformMarkupAsync();

        // Admin actualizeaza markup-ul aplicat global.
        Task UpdatePlatformMarkupAsync(decimal newMarkup);
    }
}
