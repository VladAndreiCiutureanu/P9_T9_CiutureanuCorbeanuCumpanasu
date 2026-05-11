using System.Security.Claims;
using AirlineFlightManagement.DataAccess.Repositories.Interfaces;
using AirlineFlightManagement.Models.Enums;
using AirlineFlightManagement.Services.Interfaces;
using AirlineFlightManagementWeb.Models.ViewModels.Payment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AirlineFlightManagementWeb.Controllers
{
    /// <summary>
    /// Controller pentru fluxul de plata (Coleg C).
    ///
    /// Toate actiunile cer login. Ownership-ul se verifica explicit pe
    /// fiecare actiune: pasagerul plateste / vede doar propriile tranzactii.
    /// </summary>
    [Authorize]
    public class PaymentsController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly IUnitOfWork _uow;

        public PaymentsController(IPaymentService paymentService, IUnitOfWork uow)
        {
            _paymentService = paymentService;
            _uow = uow;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  GET /Payments/Process/{reservationId}
        //  Afisare formular de plata pentru o rezervare Pending
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Process(int reservationId)
        {
            if (reservationId <= 0) return BadRequest();

            // Incarcam rezervarea cu detalii de zbor + loc pentru afisare.
            var reservation = await _uow.ReservationRepository.GetWithDetailsAsync(reservationId);
            if (reservation == null) return NotFound();

            // ─── Ownership check ────────────────────────────────────────────
            // Plata se face doar pentru propriile rezervari.
            // Spre deosebire de Details, aici NU permitem si Staff/Admin -
            // platile sunt strict actiuni ale clientului.
            var passengerId = await GetCurrentPassengerIdAsync();
            if (passengerId == null || reservation.PassengerId != passengerId)
                return Forbid();

            // ─── Status check ───────────────────────────────────────────────
            // Afisam formularul de plata doar pentru rezervari Pending.
            if (reservation.Status == ReservationStatus.ConfirmedAndPaid)
            {
                TempData["Info"] = "Aceasta rezervare a fost deja platita.";
                return RedirectToAction("Details", "Reservations", new { id = reservationId });
            }
            if (reservation.Status == ReservationStatus.Cancelled)
            {
                TempData["Error"] = "Nu se poate plati o rezervare anulata.";
                return RedirectToAction("Index", "Reservations");
            }

            var vm = new ProcessPaymentViewModel
            {
                ReservationId = reservation.ReservationId,
                AirlineName = reservation.Flight?.AirlineName ?? "—",
                Source = reservation.Flight?.Source ?? "—",
                Destination = reservation.Flight?.Destination ?? "—",
                DepartureTime = reservation.Flight?.DepartureTime ?? default,
                SeatNumber = reservation.Seat?.SeatNumber ?? "—",
                TotalPrice = reservation.TotalPrice,
                AvailablePaymentMethods = BuildPaymentMethodOptions()
            };

            return View(vm);
        }

        // ─────────────────────────────────────────────────────────────────────
        //  POST /Payments/Process
        //  Procesare plata. Suma se citeste server-side (NU din formular)
        //  ca sa nu poata fi falsificata.
        // ─────────────────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Process(ProcessPaymentViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                await RehydrateProcessViewModelAsync(vm);
                return View(vm);
            }

            // Re-load rezervarea ca sa luam suma corecta (NU din vm.TotalPrice).
            var reservation = await _uow.ReservationRepository
                .GetWithDetailsAsync(vm.ReservationId);
            if (reservation == null) return NotFound();

            // Ownership check (din nou — request-urile pot veni si direct, nu doar prin GET).
            var passengerId = await GetCurrentPassengerIdAsync();
            if (passengerId == null || reservation.PassengerId != passengerId)
                return Forbid();

            // Apel service. Suma e luata din DB, nu din input.
            var result = await _paymentService.ProcessAsync(
                vm.ReservationId,
                vm.PaymentMethod,
                reservation.TotalPrice);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Plata a esuat.");
                await RehydrateProcessViewModelAsync(vm);
                return View(vm);
            }

            // Gasim PaymentId-ul ca sa redirectam la Confirmation.
            // Service-ul intoarce TransactionId, dar pentru URL ne trebuie PaymentId.
            var payment = await _uow.PaymentRepository
                .GetByTransactionIdAsync(result.TransactionId!);
            if (payment == null)
            {
                // Caz extrem: plata a fost salvata dar nu o regasim.
                // Trimite la Index ca fallback.
                TempData["Success"] = "Plata a fost procesata cu succes.";
                return RedirectToAction("Index", "Reservations");
            }

            TempData["Success"] = "Plata a fost procesata cu succes!";
            return RedirectToAction(nameof(Confirmation), new { paymentId = payment.PaymentId });
        }

        // ─────────────────────────────────────────────────────────────────────
        //  GET /Payments/Confirmation/{paymentId}
        //  Pagina de succes (REQ-50)
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Confirmation(int paymentId)
        {
            if (paymentId <= 0) return BadRequest();

            var payment = await _uow.PaymentRepository.GetByIdAsync(paymentId);
            if (payment == null) return NotFound();

            // Pentru afisare ne trebuie Reservation + Flight + Seat.
            // GetByIdAsync (FindAsync) NU include nav properties, deci facem
            // un query suplimentar prin ReservationRepository.
            var reservation = await _uow.ReservationRepository
                .GetWithDetailsAsync(payment.ReservationId);
            if (reservation == null) return NotFound();

            // Ownership check via reservation -> passenger
            var passengerId = await GetCurrentPassengerIdAsync();
            if (passengerId == null || reservation.PassengerId != passengerId)
                return Forbid();

            var vm = new PaymentConfirmationViewModel
            {
                PaymentId = payment.PaymentId,
                TransactionId = payment.TransactionId,
                TransactionDate = payment.TransactionDate,
                Amount = payment.Amount,
                PaymentMethod = payment.PaymentMethod,
                ReservationId = reservation.ReservationId,
                AirlineName = reservation.Flight?.AirlineName ?? "—",
                Source = reservation.Flight?.Source ?? "—",
                Destination = reservation.Flight?.Destination ?? "—",
                DepartureTime = reservation.Flight?.DepartureTime ?? default,
                SeatNumber = reservation.Seat?.SeatNumber ?? "—"
            };

            return View(vm);
        }

        // ─────────────────────────────────────────────────────────────────────
        //  GET /Payments/History
        //  Istoric plati pentru pasagerul curent
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> History()
        {
            var passengerId = await GetCurrentPassengerIdAsync();
            if (passengerId == null)
                return RedirectToAction("Index", "Home");

            var payments = await _paymentService.GetHistoryAsync(passengerId.Value);

            var vm = new PaymentHistoryViewModel
            {
                Payments = payments
                    .Select(p => new PaymentHistoryItemViewModel
                    {
                        PaymentId = p.PaymentId,
                        TransactionId = p.TransactionId,
                        TransactionDate = p.TransactionDate,
                        Amount = p.Amount,
                        PaymentMethod = p.PaymentMethod,
                        Status = p.Status,
                        ReservationId = p.ReservationId,
                        // Reservation + Flight sunt incluse in repo (am adaugat Include)
                        FlightInfo = BuildFlightInfo(p)
                    })
                    .ToList()
            };

            return View(vm);
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Helper-e private
        // ─────────────────────────────────────────────────────────────────────

        // Identica cu cea din ReservationsController. Daca apar mai multe
        // duplicate, putem extrage intr-o clasa de baza ControllerBase
        // sau intr-un service helper.
        private async Task<int?> GetCurrentPassengerIdAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return null;

            var profile = await _uow.PassengerProfileRepository.GetByUserIdAsync(userId);
            return profile?.PassengerId;
        }

        // Construieste lista pentru dropdown de metode plata din enum.
        private static List<SelectListItem> BuildPaymentMethodOptions()
        {
            return Enum.GetValues<PaymentMethod>()
                .Select(pm => new SelectListItem
                {
                    Value = ((int)pm).ToString(),
                    Text = pm.ToString()
                })
                .ToList();
        }

        // La POST esuat, repopulam datele de afisare.
        private async Task RehydrateProcessViewModelAsync(ProcessPaymentViewModel vm)
        {
            vm.AvailablePaymentMethods = BuildPaymentMethodOptions();

            var reservation = await _uow.ReservationRepository
                .GetWithDetailsAsync(vm.ReservationId);
            if (reservation == null) return;

            vm.AirlineName = reservation.Flight?.AirlineName ?? "—";
            vm.Source = reservation.Flight?.Source ?? "—";
            vm.Destination = reservation.Flight?.Destination ?? "—";
            vm.DepartureTime = reservation.Flight?.DepartureTime ?? default;
            vm.SeatNumber = reservation.Seat?.SeatNumber ?? "—";
            vm.TotalPrice = reservation.TotalPrice;
        }

        // Format scurt al zborului pentru afisare in istoricul de plati.
        private static string BuildFlightInfo(AirlineFlightManagement.Models.Models.Payment p)
        {
            var flight = p.Reservation?.Flight;
            if (flight == null) return "—";

            return $"{flight.Source} → {flight.Destination}, " +
                   $"{flight.DepartureTime:dd MMM yyyy}";
        }
    }
}
