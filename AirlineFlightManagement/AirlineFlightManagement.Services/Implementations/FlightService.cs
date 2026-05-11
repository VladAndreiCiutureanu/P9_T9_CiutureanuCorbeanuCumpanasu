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

        // Cate locuri generam per clasa cand cream un zbor nou din API.
        // 30 e suficient pentru testare; in productie ar trebui calculat
        // din Aircraft.MaxCapacity si distributie proportionala per clasa.
        private const int SEATS_PER_CLASS = 30;

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
                // Migrare retroactiva: pentru zborurile cached fara locuri
                // (cauzate de cod vechi care nu genera FlightSeats), le populam acum.
                var needSeatInit = localFlights
                    .Where(f => f.FlightClasses.Any() && !f.FlightSeats.Any())
                    .Select(f => f.FlightId)
                    .ToList();

                if (needSeatInit.Any())
                {
                    foreach (var flightId in needSeatInit)
                    {
                        await EnsureFlightHasSeatsAsync(flightId);
                    }

                    // Re-citim ca sa avem locurile nou-create in DTO
                    localFlights = (await _uow.FlightRepository.GetAllAsync(
                        filter: f => f.Source == source
                                  && f.Destination == destination
                                  && f.DepartureTime.Date == date.Date,
                        tracked: false,
                        f => f.FlightClasses,
                        f => f.FlightSeats)).ToList();
                }

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

            // 4. Re-citim din DB ca sa avem FlightId si FlightClassId populate.
            //    Asta permite afisarea butonului "Rezerva" din prima cautare.
            var persistedFlights = (await _uow.FlightRepository.GetAllAsync(
                filter: f => f.Source == source
                          && f.Destination == destination
                          && f.DepartureTime.Date == date.Date,
                tracked: false,
                f => f.FlightClasses,
                f => f.FlightSeats)).ToList();

            return await BuildResultsWithMarkupAsync(persistedFlights, fromLocalDb: true);
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
                    // Genereaza locuri in-memory inainte de save — vor fi salvate
                    // in cascada impreuna cu Flight si FlightClasses.
                    GenerateSeatsInMemory(flight);
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

        // Genereaza FlightSeats pentru fiecare FlightClass a unui zbor NOU
        // (entitate ne-persistata inca). EF le va cascada-salva impreuna
        // cu Flight + FlightClasses la AddAsync + SaveAsync.
        private static void GenerateSeatsInMemory(Flight flight)
        {
            foreach (var flightClass in flight.FlightClasses)
            {
                char prefix = flightClass.ClassName.Length > 0
                    ? char.ToUpper(flightClass.ClassName[0])
                    : 'X';

                for (int i = 1; i <= SEATS_PER_CLASS; i++)
                {
                    flight.FlightSeats.Add(new FlightSeat
                    {
                        // Navigation property — EF va seta FlightClassId dupa save
                        FlightClass = flightClass,
                        SeatNumber = $"{prefix}{i:D2}",
                        IsAvailable = true
                    });
                }
            }
        }

        // Migrare retroactiva: pentru zborurile deja cached in DB fara locuri,
        // genereaza locurile la prima cautare.
        private async Task EnsureFlightHasSeatsAsync(int flightId)
        {
            // Verificare rapida — sarim daca exista deja locuri.
            int existing = await _uow.FlightSeatRepository.CountAvailableAsync(flightId)
                         + await _uow.FlightSeatRepository.CountSoldAsync(flightId);
            if (existing > 0) return;

            // Incarcam zborul TRACKED cu FlightClasses pentru a putea modifica.
            var flight = await _uow.FlightRepository.GetAsync(
                f => f.FlightId == flightId,
                tracked: true,
                f => f.FlightClasses);

            if (flight == null || !flight.FlightClasses.Any()) return;

            foreach (var flightClass in flight.FlightClasses)
            {
                char prefix = flightClass.ClassName.Length > 0
                    ? char.ToUpper(flightClass.ClassName[0])
                    : 'X';

                for (int i = 1; i <= SEATS_PER_CLASS; i++)
                {
                    flight.FlightSeats.Add(new FlightSeat
                    {
                        FlightClassId = flightClass.FlightClassId,
                        SeatNumber = $"{prefix}{i:D2}",
                        IsAvailable = true
                    });
                }
            }

            await _uow.SaveAsync();
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
