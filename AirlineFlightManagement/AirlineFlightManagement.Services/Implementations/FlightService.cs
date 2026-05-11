using AirlineFlightManagement.DataAccess.Repositories.Interfaces;
using AirlineFlightManagement.Models.Enums;
using AirlineFlightManagement.Models.Models;
using AirlineFlightManagement.Services.Contracts;
using AirlineFlightManagement.Services.Interfaces;

namespace AirlineFlightManagement.Services.Implementations
{
    /// <summary>
    /// Implementare combinata:
    ///   - Logica DB-first + fallback API (originar Coleg B / feat/api)
    ///   - Markup aplicat la nivel de DTO (NU pe entitatea EF — important!)
    ///   - Operatii admin pentru pret/anulare (REQ-39, BR-3)
    /// </summary>
    public class FlightService : IFlightService
    {
        private readonly ISerpApiClient _serpApi;
        private readonly IMarkupService _markup;
        private readonly IUnitOfWork _uow;

        // Capacitate default daca trebuie sa cream o aeronava implicita
        // pentru zborurile noi venite din API.
        private const int DEFAULT_AIRCRAFT_CAPACITY = 200;

        public FlightService(
            ISerpApiClient serpApi,
            IMarkupService markup,
            IUnitOfWork uow)
        {
            _serpApi = serpApi;
            _markup = markup;
            _uow = uow;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  SearchAsync — DB-first, API fallback, markup pe DTO
        // ─────────────────────────────────────────────────────────────────────
        public async Task<IEnumerable<FlightSearchResult>> SearchAsync(
            string source,
            string destination,
            DateTime date)
        {
            // 1. Cautam intai in DB local (cu Include pe FlightClasses si FlightSeats
            //    pentru a putea construi rezultatele fara N+1 queries).
            var localFlights = (await _uow.FlightRepository.GetAllAsync(
                filter: f => f.Source == source
                          && f.Destination == destination
                          && f.DepartureTime.Date == date.Date,
                tracked: false,
                f => f.FlightClasses,
                f => f.FlightSeats)).ToList();

            if (localFlights.Any())
            {
                return await BuildResultsWithMarkupAsync(localFlights, fromLocalDb: true);
            }

            // 2. DB-ul nu are nimic — interogam SerpAPI
            var apiFlights = (await _serpApi.SearchFlightsAsync(source, destination, date))
                .ToList();

            if (!apiFlights.Any())
            {
                return Enumerable.Empty<FlightSearchResult>();
            }

            // 3. Salvam in DB cele care nu exista deja (REQ-24 cache local)
            await PersistNewFlightsAsync(apiFlights);

            return await BuildResultsWithMarkupAsync(apiFlights, fromLocalDb: false);
        }

        // ─────────────────────────────────────────────────────────────────────
        //  GetWithDetailsAsync — detalii complete zbor (folosit pe pagina booking)
        // ─────────────────────────────────────────────────────────────────────
        public Task<Flight?> GetWithDetailsAsync(int flightId)
        {
            return _uow.FlightRepository.GetWithDetailsAsync(flightId);
        }

        // ─────────────────────────────────────────────────────────────────────
        //  ImportFromApiAsync — pentru MVP, doar verifica daca exista.
        //  Pentru "import direct" complet, ar trebui extins SerpAPI cu un
        //  GetByExternalIdAsync. Workaround: foloseste SearchAsync cu criteriile
        //  zborului, care va popula DB automat la prima cautare.
        // ─────────────────────────────────────────────────────────────────────
        public async Task<Flight> ImportFromApiAsync(string externalApiId)
        {
            var existing = await _uow.FlightRepository.GetByExternalApiIdAsync(externalApiId);
            if (existing != null) return existing;

            throw new NotSupportedException(
                "Import direct dupa ExternalApiId nu este suportat pentru MVP. " +
                "Apelati SearchAsync cu sursa/destinatia/data corespunzatoare — " +
                "zborul va fi salvat automat in DB la prima cautare.");
        }

        // ─────────────────────────────────────────────────────────────────────
        //  UpdateClassPriceAsync — admin modifica pretul (BR-3)
        // ─────────────────────────────────────────────────────────────────────
        public async Task UpdateClassPriceAsync(int flightClassId, decimal newPrice)
        {
            if (newPrice <= 0)
                throw new ArgumentException(
                    "Pretul trebuie sa fie pozitiv.",
                    nameof(newPrice));

            var fc = await _uow.FlightClassRepository.GetByIdAsync(flightClassId)
                ?? throw new KeyNotFoundException(
                    $"Clasa de zbor inexistenta: ID {flightClassId}.");

            fc.Price = newPrice;
            _uow.FlightClassRepository.Update(fc);
            await _uow.SaveAsync();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  CancelFlightAsync — admin anuleaza un zbor (BR-3, REQ-39)
        // ─────────────────────────────────────────────────────────────────────
        public async Task CancelFlightAsync(int flightId)
        {
            var flight = await _uow.FlightRepository.GetByIdAsync(flightId)
                ?? throw new KeyNotFoundException(
                    $"Zbor inexistent: ID {flightId}.");

            // REQ-39: refuzam anularea daca exista rezervari active
            bool hasActive = await _uow.FlightRepository
                .HasActiveReservationsAsync(flightId);
            if (hasActive)
            {
                throw new InvalidOperationException(
                    "Zborul are rezervari active si nu poate fi anulat. " +
                    "Anulati intai rezervarile sau contactati pasagerii.");
            }

            flight.Status = FlightStatus.Cancelled;
            _uow.FlightRepository.Update(flight);
            await _uow.SaveAsync();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Helper-e private
        // ─────────────────────────────────────────────────────────────────────

        // Persista in DB zborurile noi venite din API (REQ-24).
        // Asigura ca exista o aeronava implicita pentru a satisface FK-ul.
        private async Task PersistNewFlightsAsync(List<Flight> apiFlights)
        {
            var aircrafts = (await _uow.AircraftRepository.GetAllAsync(tracked: false))
                .ToList();
            var defaultAircraft = aircrafts.FirstOrDefault();

            if (defaultAircraft == null)
            {
                defaultAircraft = new Aircraft
                {
                    ModelName = "External API Default Aircraft",
                    MaxCapacity = DEFAULT_AIRCRAFT_CAPACITY
                };
                await _uow.AircraftRepository.AddAsync(defaultAircraft);
                await _uow.SaveAsync();
            }

            foreach (var flight in apiFlights)
            {
                var existing = await _uow.FlightRepository
                    .GetByExternalApiIdAsync(flight.ExternalApiId);
                if (existing == null)
                {
                    flight.AircraftId = defaultAircraft.AircraftId;
                    await _uow.FlightRepository.AddAsync(flight);
                }
            }

            try
            {
                await _uow.SaveAsync();
            }
            catch (Exception ex)
            {
                // Daca salvarea esueaza (de exemplu duplicate index pe ExternalApiId
                // intr-un race condition), nu vrem sa stricam afisarea rezultatelor.
                Console.WriteLine($"[FlightService] Eroare la salvare zboruri API: {ex.Message}");
            }
        }

        // Construieste DTO-urile FlightSearchResult, aplicand markup-ul pe DTO
        // (NU pe entitatile EF — esential ca pretul real din DB sa ramana intact).
        private async Task<IEnumerable<FlightSearchResult>> BuildResultsWithMarkupAsync(
            List<Flight> flights,
            bool fromLocalDb)
        {
            var markup = await _markup.GetPlatformMarkupAsync();

            return flights.Select(f => new FlightSearchResult
            {
                ExternalApiId = f.ExternalApiId,
                LocalFlightId = fromLocalDb ? f.FlightId : (int?)null,
                AirlineName = f.AirlineName,
                Source = f.Source,
                Destination = f.Destination,
                DepartureTime = f.DepartureTime,
                ArrivalTime = f.ArrivalTime,
                AvailableSeats = f.FlightSeats?.Count(s => s.IsAvailable) ?? 0,
                IsFromLocalDb = fromLocalDb,
                Classes = f.FlightClasses?.Select(fc => new FlightClassInfo
                {
                    FlightClassId = fromLocalDb ? fc.FlightClassId : (int?)null,
                    ClassName = fc.ClassName,
                    Price = fc.Price + markup,
                    AvailableSeats = f.FlightSeats?
                        .Count(s => s.FlightClassId == fc.FlightClassId && s.IsAvailable) ?? 0
                }).ToList() ?? new List<FlightClassInfo>()
            }).ToList();
        }
    }
}
