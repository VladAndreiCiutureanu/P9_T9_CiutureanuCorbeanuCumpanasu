using AirlineFlightManagement.Models.Models;
using AirlineFlightManagement.Services.Contracts;

namespace AirlineFlightManagement.Services.Interfaces
{
    public interface IFlightService
    {
        // REQ-22, REQ-26 — search DB-first + fallback API, dedupe dupa ExternalApiId
        Task<IEnumerable<FlightSearchResult>> SearchAsync(
            string source,
            string destination,
            DateTime date);

        // Detalii pentru pagina de booking (Flight + FlightClasses + FlightSeats)
        Task<Flight?> GetWithDetailsAsync(int flightId);

        // Cand admin doreste sa modifice un zbor care nu exista inca local,
        // se importa intai din API si se persista cu prețurile API ca baza.
        // REQ-24 (cache local), REQ-27 (premergator pentru update price)
        Task<Flight> ImportFromApiAsync(string externalApiId);

        // BR-3 — doar Admin poate modifica pretul. Daca zborul nu e local, importa intai.
        Task UpdateClassPriceAsync(int flightClassId, decimal newPrice);

        // BR-3 — admin anuleaza un zbor. REQ-39 verifica intai daca exista rezervari active.
        Task CancelFlightAsync(int flightId);
    }
}
