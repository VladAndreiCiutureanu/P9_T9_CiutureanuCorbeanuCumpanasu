using AirlineFlightManagement.DataAccess.Repositories.Interfaces;
using AirlineFlightManagement.Models.Enums;
using AirlineFlightManagement.Models.Models;
using AirlineFlightManagement.Services.Contracts;
using AirlineFlightManagement.Services.Interfaces;

namespace AirlineFlightManagement.Services.Implementations
{
    /// <summary>
    /// Implementeaza fluxul de plata. Pentru proiectul curent NU integram
    /// un gateway extern (Stripe / PayPal): simulam succesul direct.
    ///
    /// Acoperire SRS:
    ///   - REQ-42 (PaymentMethod selectabil din lista predefinita)
    ///   - REQ-43 (data tranzactiei)
    ///   - REQ-45 (Reservation.Status -> ConfirmedAndPaid)
    ///   - REQ-47 (TransactionId unic)
    ///   - 5.2   (Transaction Rollback la nivel de DB)
    /// </summary>
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _uow;

        public PaymentService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  ProcessAsync — fluxul principal de plata
        // ─────────────────────────────────────────────────────────────────────
        public async Task<PaymentResult> ProcessAsync(
            int reservationId,
            PaymentMethod method,
            decimal amount)
        {
            // ─── PAS 1: Validare argumente de baza ────────────────────────────
            if (reservationId <= 0)
                return PaymentResult.Fail("Reservation ID invalid.");
            if (amount <= 0)
                return PaymentResult.Fail("Suma platita trebuie sa fie pozitiva.");

            // ─── PAS 2: Incarcam rezervarea TRACKED (urmeaza s-o modificam) ──
            // GetWithDetailsAsync din repo foloseste AsNoTracking, deci pentru
            // update folosim metoda generica GetAsync cu tracked: true.
            // Includem Flight pentru afisare in mesajele de eroare daca e cazul.
            var reservation = await _uow.ReservationRepository.GetAsync(
                filter: r => r.ReservationId == reservationId,
                tracked: true,
                r => r.Flight!);

            if (reservation == null)
                return PaymentResult.Fail($"Rezervare inexistenta: ID {reservationId}.");

            // ─── PAS 3: Validari de business ──────────────────────────────────
            // Plata se face doar pentru rezervari in starea Pending.
            // Daca cineva incearca sa plateasca de doua ori sau sa plateasca
            // o rezervare anulata, refuzam clar.
            if (reservation.Status == ReservationStatus.ConfirmedAndPaid)
                return PaymentResult.Fail("Rezervarea a fost deja platita.");
            if (reservation.Status == ReservationStatus.Cancelled)
                return PaymentResult.Fail("Nu se poate plati o rezervare anulata.");

            // Suma platita trebuie sa fie EXACT egala cu pretul rezervarii
            // (REQ-38: pretul calculat la creare). Comparatia pe decimal
            // e exacta, fara probleme de floating point.
            if (amount != reservation.TotalPrice)
                return PaymentResult.Fail(
                    $"Suma platita ({amount}) nu corespunde pretului " +
                    $"rezervarii ({reservation.TotalPrice}).");

            // ─── PAS 4: Tranzactie atomica ────────────────────────────────────
            // Desi nu mai chemam un gateway extern, pastram tranzactia pentru
            // ca facem doua modificari (insert Payment + update Reservation)
            // care trebuie sa fie consistente. Daca SaveAsync esueaza din alt
            // motiv (ex: DB cade), rollback automat la dispose.
            using var tx = await _uow.BeginTransactionAsync();

            // ─── PAS 5: Generam TransactionId unic ────────────────────────────
            // REQ-47: fiecare plata are un identificator unic pentru audit.
            // Folosim Guid cu format "N" (32 caractere hex, fara liniute).
            // Indexul unique pe coloana TransactionId din DB (vezi DbContext)
            // ne protejeaza chiar daca apar coliziuni teoretice.
            var transactionId = Guid.NewGuid().ToString("N");

            // ─── PAS 6: Cream entitatea Payment ───────────────────────────────
            // In productie:
            //   Status = Pending  → apel HTTP la Stripe → on success → Completed
            // La noi (simulare): direct Completed.
            var payment = new Payment
            {
                ReservationId = reservationId,
                TransactionId = transactionId,
                PaymentMethod = method,
                TransactionDate = DateTime.UtcNow,   // REQ-43
                Amount = amount,
                Status = PaymentStatus.Completed
            };
            await _uow.PaymentRepository.AddAsync(payment);

            // ─── PAS 7: Promovam rezervarea ───────────────────────────────────
            // REQ-45: dupa plata reusita, status devine ConfirmedAndPaid.
            // Asta o face vizibila in raportul de manifest (REQ-35) si in
            // numaratorile pentru BR-1.
            reservation.Status = ReservationStatus.ConfirmedAndPaid;
            _uow.ReservationRepository.Update(reservation);

            // ─── PAS 8: Persistam si commitam ─────────────────────────────────
            await _uow.SaveAsync();
            await tx.CommitAsync();

            return PaymentResult.Ok(transactionId);
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Read-only helpers
        // ─────────────────────────────────────────────────────────────────────

        // Toate platile asociate unei rezervari (de regula doar una, dar
        // tinem semnatura plurala pentru cazuri viitoare: refund, retry).
        public async Task<IEnumerable<Payment>> GetByReservationAsync(int reservationId)
        {
            if (reservationId <= 0)
                throw new ArgumentException("Reservation ID invalid.", nameof(reservationId));

            return await _uow.PaymentRepository.GetByReservationAsync(reservationId);
        }

        // Istoric plati per pasager (pagina "Platile mele" / audit).
        public async Task<IEnumerable<Payment>> GetHistoryAsync(int passengerId)
        {
            if (passengerId <= 0)
                throw new ArgumentException("Passenger ID invalid.", nameof(passengerId));

            return await _uow.PaymentRepository.GetByPassengerIdAsync(passengerId);
        }
    }
}
