using AirlineFlightManagement.DataAccess.Repositories.Interfaces;
using AirlineFlightManagement.Models.Enums;
using AirlineFlightManagement.Models.Models;
using AirlineFlightManagement.Services.Interfaces;

namespace AirlineFlightManagement.Services.Implementations
{
    /// <summary>
    /// Implementeaza fluxul de rezervare cu enforcement pentru:
    ///   - REQ-25 (verificare disponibilitate via API extern)
    ///   - REQ-30 .. REQ-41 (creare/anulare/listare rezervari)
    ///   - BR-1 (overbooking prevention)
    ///   - BR-2 (24h cancellation window)
    ///   - BR-4 (un pasager nu poate avea 2 rezervari active pe acelasi zbor)
    /// </summary>
    public class ReservationService : IReservationService
    {
        private readonly IUnitOfWork _uow;
        private readonly ISerpApiClient _serpApiClient;

        // Numarul minim de ore inainte de plecare in care un pasager
        // mai poate anula independent rezervarea (BR-2).
        private const int CANCELLATION_WINDOW_HOURS = 24;

        public ReservationService(IUnitOfWork uow, ISerpApiClient serpApiClient)
        {
            _uow = uow;
            _serpApiClient = serpApiClient;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  CreateAsync — fluxul critic, cea mai grea metoda din proiect.
        //  Aceasta metoda combina: verificari de business, apel HTTP la API
        //  extern, modificare seat, creare rezervare. Toate trebuie sa fie
        //  atomice pentru a respecta BR-1 / BR-4.
        // ─────────────────────────────────────────────────────────────────────
        public async Task<Reservation> CreateAsync(
            int passengerId,
            int flightId,
            int flightClassId,
            int? seatId = null)
        {
            // ─── PAS 1: Validare argumente de baza ────────────────────────────
            // Aruncam exceptie din timp daca apelantul a trimis input prost,
            // ca sa nu pornim tranzactii sau apeluri HTTP degeaba.
            if (passengerId <= 0)
                throw new ArgumentException("Passenger ID invalid.", nameof(passengerId));
            if (flightId <= 0)
                throw new ArgumentException("Flight ID invalid.", nameof(flightId));
            if (flightClassId <= 0)
                throw new ArgumentException("Flight class ID invalid.", nameof(flightClassId));

            // ─── PAS 2: BR-4 (early check) ────────────────────────────────────
            // Aceeasi regula e enforced si la nivel de DB printr-un index unique
            // filtrat (vezi ApplicationDbContext.OnModelCreating). Verificarea
            // aici ne permite sa returnam un mesaj curat in loc de exceptia DB.
            bool hasActiveReservation = await _uow.ReservationRepository
                .HasActiveReservationAsync(passengerId, flightId);
            if (hasActiveReservation)
                throw new InvalidOperationException(
                    "Aveti deja o rezervare activa pentru acest zbor.");

            // ─── PAS 3: Incarcam zborul cu detalii pentru BR-1 ────────────────
            // GetWithDetailsAsync include Aircraft (cu MaxCapacity) si
            // FlightClasses. Daca zborul nu exista local, refuzam booking-ul:
            // logica de "cache local la booking" e responsabilitatea
            // FlightService / IFlightService.ImportFromApiAsync.
            var flight = await _uow.FlightRepository.GetWithDetailsAsync(flightId)
                ?? throw new KeyNotFoundException(
                    $"Zbor inexistent in DB local: ID {flightId}.");

            // Daca zborul a fost anulat de admin (BR-3), refuzam.
            if (flight.Status != FlightStatus.Scheduled)
                throw new InvalidOperationException(
                    $"Zborul nu mai e disponibil pentru rezervare (status: {flight.Status}).");

            // Aircraft trebuie sa fie populat pentru BR-1.
            // GetWithDetailsAsync il include, dar punem o asertie defensiva.
            if (flight.Aircraft == null)
                throw new InvalidOperationException(
                    "Zborul nu are aeronava asignata. Contactati administratorul.");

            // ─── PAS 4: REQ-25 — verificam disponibilitatea via API ───────────
            // Apelul la Amadeus se face inainte de tranzactie (e potential lent
            // si poate da timeout - 5.1 zice max 5 secunde). Daca API-ul cade
            // sau zice ca zborul a fost anulat, refuzam booking-ul (5.2 fail-safe).
            bool availableInApi = await _serpApiClient
                .VerifyAvailabilityAsync(flight.ExternalApiId);
            if (!availableInApi)
                throw new InvalidOperationException(
                    "Zborul a fost anulat sau este full conform API-ului extern.");

            // ─── PAS 5: Incarcam clasa pentru pret ────────────────────────────
            // REQ-38: pretul rezervarii = pretul clasei selectate.
            var flightClass = await _uow.FlightClassRepository
                .GetByIdAsync(flightClassId)
                ?? throw new KeyNotFoundException(
                    $"Clasa de zbor inexistenta: ID {flightClassId}.");

            // Sanity check: clasa trebuie sa apartina zborului cerut.
            if (flightClass.FlightId != flightId)
                throw new InvalidOperationException(
                    "Clasa selectata nu apartine zborului indicat.");

            // ─── PAS 6: Incepem tranzactia ────────────────────────────────────
            // De aici inainte, toate operatiile pe DB sunt sub aceeasi tranzactie.
            // Daca ceva esueaza, rollback automat la Dispose (using).
            using var tx = await _uow.BeginTransactionAsync();

            // ─── PAS 7: BR-1 — verificare capacitate (in tranzactie) ──────────
            // Numaram rezervarile ConfirmedAndPaid pentru zbor.
            // Verificarea aici e duplicata cu seat.IsAvailable (care e si el
            // un proxy pentru "vandut"), dar avand ambele protectii reduce
            // riscul de overbooking sub concurenta.
            int sold = await _uow.ReservationRepository
                .CountConfirmedForFlightAsync(flightId);
            if (sold >= flight.Aircraft.MaxCapacity)
                throw new InvalidOperationException(
                    "Zbor full. Nu se mai accepta rezervari.");

            // ─── PAS 8: Selectam locul ────────────────────────────────────────
            // Daca apelantul a cerut un loc specific, il incarcam tracked
            // (urmeaza sa il modificam: IsAvailable = false).
            // Daca nu, luam primul disponibil din clasa (REQ-37).
            FlightSeat? seat;
            if (seatId.HasValue)
            {
                // tracked: true pentru ca urmeaza sa il modificam.
                seat = await _uow.FlightSeatRepository.GetAsync(
                    s => s.FlightSeatId == seatId.Value,
                    tracked: true);
            }
            else
            {
                // tracked: true (overload nou pe care l-am adaugat).
                var available = await _uow.FlightSeatRepository
                    .GetAvailableAsync(flightId, flightClassId, tracked: true);
                seat = available.FirstOrDefault();
            }

            // REQ-37: refuzam daca nu e niciun loc disponibil sau locul cerut e ocupat.
            if (seat == null)
                throw new InvalidOperationException(
                    "Niciun loc disponibil in clasa selectata.");
            if (!seat.IsAvailable)
                throw new InvalidOperationException(
                    "Locul selectat este deja rezervat.");
            if (seat.FlightId != flightId)
                throw new InvalidOperationException(
                    "Locul selectat nu apartine zborului indicat.");

            // ─── PAS 9: Marcam locul ca ocupat ────────────────────────────────
            // REQ-32: la confirmarea rezervarii, locul nu mai e disponibil.
            // Update-ul nu e persistat inca - va merge in DB la SaveAsync.
            seat.IsAvailable = false;
            _uow.FlightSeatRepository.Update(seat);

            // ─── PAS 10: Cream entitatea Reservation ──────────────────────────
            // REQ-31: o rezervare = un pasager + un zbor.
            // REQ-33: timestamp generat de noi (UtcNow pentru consistenta).
            // REQ-38: TotalPrice copiat din FlightClass.Price.
            // Status initial: Pending (devine ConfirmedAndPaid dupa plata).
            var reservation = new Reservation
            {
                PassengerId = passengerId,
                FlightId = flightId,
                FlightSeatId = seat.FlightSeatId,
                TotalPrice = flightClass.Price,
                ReservationTimeStamp = DateTime.UtcNow,
                Status = ReservationStatus.Pending
            };
            await _uow.ReservationRepository.AddAsync(reservation);

            // ─── PAS 11: Persistam si commitam ────────────────────────────────
            // SaveAsync trimite la DB ambele modificari (seat update + reservation
            // insert) intr-un singur batch. Tranzactia explicita garanteaza ca
            // si verificarile de mai sus (PAS 7) au vazut aceeasi snapshot.
            await _uow.SaveAsync();
            await tx.CommitAsync();

            return reservation;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  CancelAsync — REQ-40, REQ-41, BR-2.
        //  Fara tranzactie explicita — un singur SaveAsync e atomic by default.
        // ─────────────────────────────────────────────────────────────────────
        public async Task CancelAsync(int reservationId, int passengerId)
        {
            if (reservationId <= 0)
                throw new ArgumentException("Reservation ID invalid.", nameof(reservationId));
            if (passengerId <= 0)
                throw new ArgumentException("Passenger ID invalid.", nameof(passengerId));

            // ─── Incarcam rezervarea cu Flight si Seat (TRACKED!) ─────────────
            // GetWithDetailsAsync din repo foloseste AsNoTracking. Ca sa modificam,
            // facem un query tracked direct via metoda generica GetAsync.
            var reservation = await _uow.ReservationRepository.GetAsync(
                filter: r => r.ReservationId == reservationId,
                tracked: true,
                r => r.Flight!,
                r => r.Seat!);

            if (reservation == null)
                throw new KeyNotFoundException(
                    $"Rezervare inexistenta: ID {reservationId}.");

            // ─── Verificare ownership ─────────────────────────────────────────
            // Doar pasagerul care a facut rezervarea o poate anula
            // (admin-ul ar folosi alta metoda dedicata, daca apare nevoia).
            if (reservation.PassengerId != passengerId)
                throw new UnauthorizedAccessException(
                    "Nu puteti anula rezervari care nu va apartin.");

            // ─── Idempotenta: deja anulata ────────────────────────────────────
            if (reservation.Status == ReservationStatus.Cancelled)
                throw new InvalidOperationException(
                    "Rezervarea este deja anulata.");

            // ─── BR-2: minim 24h inainte de plecare ───────────────────────────
            // Folosim UtcNow pentru consistenta cu ReservationTimeStamp si
            // cu DepartureTime stocat in DB.
            if (reservation.Flight == null)
                throw new InvalidOperationException(
                    "Datele zborului nu pot fi incarcate.");

            var hoursUntilDeparture =
                (reservation.Flight.DepartureTime - DateTime.UtcNow).TotalHours;
            if (hoursUntilDeparture < CANCELLATION_WINDOW_HOURS)
                throw new InvalidOperationException(
                    $"Anularea independenta e permisa doar cu minim " +
                    $"{CANCELLATION_WINDOW_HOURS}h inainte de plecare.");

            // ─── Modificari ───────────────────────────────────────────────────
            // 1. Marcam rezervarea ca anulata.
            // 2. Eliberam locul (REQ-41) — il poate rezerva alt pasager.
            //
            // NOTA: pentru o plata Completed asociata, in productie ar trebui
            // sa initializam un refund. Out of scope pentru MVP.
            reservation.Status = ReservationStatus.Cancelled;

            if (reservation.Seat != null)
            {
                reservation.Seat.IsAvailable = true;
                _uow.FlightSeatRepository.Update(reservation.Seat);
            }

            _uow.ReservationRepository.Update(reservation);
            await _uow.SaveAsync();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Read-only helpers — deleaga la repository
        // ─────────────────────────────────────────────────────────────────────

        // REQ-36 — istoric rezervari pentru un pasager
        public async Task<IEnumerable<Reservation>> GetHistoryAsync(int passengerId)
        {
            if (passengerId <= 0)
                throw new ArgumentException("Passenger ID invalid.", nameof(passengerId));

            return await _uow.ReservationRepository.GetByPassengerAsync(passengerId);
        }

        // REQ-35 — manifest pasageri pentru un zbor (acces Staff/Admin
        // verificat la nivel de Controller via [Authorize(Roles = ...)]).
        public async Task<IEnumerable<Reservation>> GetManifestAsync(int flightId)
        {
            if (flightId <= 0)
                throw new ArgumentException("Flight ID invalid.", nameof(flightId));

            return await _uow.ReservationRepository.GetManifestAsync(flightId);
        }

        // Detalii complete pentru pagina de confirmare / detaliu rezervare.
        public async Task<Reservation?> GetWithDetailsAsync(int reservationId)
        {
            if (reservationId <= 0)
                throw new ArgumentException("Reservation ID invalid.", nameof(reservationId));

            return await _uow.ReservationRepository.GetWithDetailsAsync(reservationId);
        }
    }
}
