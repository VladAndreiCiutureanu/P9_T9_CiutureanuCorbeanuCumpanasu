using System.Security.Claims;
using AirlineFlightManagement.DataAccess.Repositories.Interfaces;
using AirlineFlightManagement.Models.Enums;
using AirlineFlightManagement.Services.Interfaces;
using AirlineFlightManagementWeb.Models.ViewModels.Reservation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AirlineFlightManagementWeb.Controllers
{
    /// <summary>
    /// Controller pentru fluxul de rezervari (Coleg C).
    ///
    /// Toate actiunile cer login. Manifest cere si rolul Staff/Administrator.
    /// </summary>
    [Authorize]
    public class ReservationsController : Controller
    {
        private readonly IReservationService _reservationService;
        private readonly IMarkupService _markupService;
        private readonly IUnitOfWork _uow;

        // Numar de ore de la plecare in care se mai poate anula (BR-2).
        // Trebuie sa fie aceeasi valoare cu cea din ReservationService.
        private const int CANCELLATION_WINDOW_HOURS = 24;

        public ReservationsController(
            IReservationService reservationService,
            IMarkupService markupService,
            IUnitOfWork uow)
        {
            _reservationService = reservationService;
            _markupService = markupService;
            _uow = uow;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  GET /Reservations
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var passengerId = await GetCurrentPassengerIdAsync();
            if (passengerId == null)
                return RedirectToAction("Index", "Home");

            var reservations = await _reservationService.GetHistoryAsync(passengerId.Value);

            var vm = new MyReservationsViewModel
            {
                Reservations = reservations
                    .OrderByDescending(r => r.ReservationTimeStamp)
                    .Select(r => new ReservationListItemViewModel
                    {
                        ReservationId = r.ReservationId,
                        AirlineName = r.Flight?.AirlineName ?? "—",
                        Source = r.Flight?.Source ?? "—",
                        Destination = r.Flight?.Destination ?? "—",
                        DepartureTime = r.Flight?.DepartureTime ?? default,
                        SeatNumber = r.Seat?.SeatNumber ?? "—",
                        TotalPrice = r.TotalPrice,
                        Status = r.Status,
                        CanCancel = r.Status != ReservationStatus.Cancelled
                                 && (r.Flight?.DepartureTime ?? DateTime.MinValue)
                                        > DateTime.UtcNow.AddHours(CANCELLATION_WINDOW_HOURS),
                        CanPay = r.Status == ReservationStatus.Pending
                    })
                    .ToList()
            };

            return View(vm);
        }

        // ─────────────────────────────────────────────────────────────────────
        //  GET /Reservations/Create?flightId=5
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Create(int flightId, decimal minPrice = 0)
        {
            if (flightId <= 0)
                return BadRequest("FlightId invalid.");

            var flight = await _uow.FlightRepository.GetWithDetailsAsync(flightId);
            if (flight == null)
                return NotFound();

            if (flight.Status != FlightStatus.Scheduled)
            {
                TempData["Error"] = "The flight is not available for reservation.";
                return RedirectToAction("Index", "Home");
            }

            // Aplicam markup-ul si in afisarea claselor pentru consistenta cu Search:
            // pretul vazut pe pagina de cautare = pretul aici = pretul platit.
            var markup = await _markupService.GetPlatformMarkupAsync();

            var vm = new CreateReservationViewModel
            {
                FlightId = flight.FlightId,
                AirlineName = flight.AirlineName,
                Source = flight.Source,
                Destination = flight.Destination,
                DepartureTime = flight.DepartureTime,
                ArrivalTime = flight.ArrivalTime,
                MinPrice = minPrice,

                AvailableClasses = flight.FlightClasses
                    .Select(fc => new SelectListItem
                    {
                        Value = fc.FlightClassId.ToString(),
                        Text = $"{fc.ClassName} — {(fc.Price + markup):C}"
                    })
                    .ToList(),

                // Trimitem TOATE scaunele (libere si ocupate) catre view ca sa
                // afiseze layout-ul complet de avion. IsAvailable controleaza
                // daca butonul e click-abil (verde) sau gri (occupied).
                AvailableSeats = flight.FlightSeats
                    .Select(s => new AvailableSeatViewModel
                    {
                        FlightSeatId = s.FlightSeatId,
                        SeatNumber = s.SeatNumber,
                        FlightClassId = s.FlightClassId,
                        IsAvailable = s.IsAvailable,
                        // ClassName populat dintr-un dictionar local pentru evitare N+1
                        ClassName = flight.FlightClasses
                            .FirstOrDefault(fc => fc.FlightClassId == s.FlightClassId)
                            ?.ClassName ?? "—"
                    })
                    // Ordonarea pe ID pastreaza ordinea de insertie in DB
                    .OrderBy(s => s.FlightSeatId)
                    .ToList()
            };

            return View(vm);
        }

        // ─────────────────────────────────────────────────────────────────────
        //  POST /Reservations/Create
        //  Procesare formular booking
        // ─────────────────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateReservationViewModel vm)
        {
            // 1. Validare DataAnnotations
            if (!ModelState.IsValid)
            {
                await RehydrateCreateViewModelAsync(vm);
                return View(vm);
            }

            // 2. Identifica pasagerul curent
            var passengerId = await GetCurrentPassengerIdAsync();
            if (passengerId == null)
            {
                ModelState.AddModelError(string.Empty, "Profil pasager negasit. Reincercati login-ul.");
                await RehydrateCreateViewModelAsync(vm);
                return View(vm);
            }

            // 3. Apel service — toata logica de business (BR-1, BR-4, REQ-25) e acolo
            try
            {
                var reservation = await _reservationService.CreateAsync(
                    passengerId.Value, vm.FlightId, vm.FlightClassId, vm.SeatId);

                // REQ-50: notificare succes
                // Dupa creare → catre pagina de detalii (de unde poate plati)
                return RedirectToAction(nameof(Details), new { id = reservation.ReservationId });
            }
            catch (InvalidOperationException ex)
            {
                // Erori de business (BR-4, BR-1, API unavailable etc.)
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            // Pe eroare: re-populeaza listele de afisare si returneaza View-ul
            await RehydrateCreateViewModelAsync(vm);
            return View(vm);
        }

        // ─────────────────────────────────────────────────────────────────────
        //  GET /Reservations/Details/5
        //  Detalii rezervare (verifica ownership)
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0) return BadRequest();

            var reservation = await _reservationService.GetWithDetailsAsync(id);
            if (reservation == null) return NotFound();

            // Ownership: pasagerul vede doar propriile rezervari.
            // Staff/Admin pot vedea oricare (verificare in plus).
            var passengerId = await GetCurrentPassengerIdAsync();
            bool isStaff = User.IsInRole("Administrator") || User.IsInRole("Staff");
            if (reservation.PassengerId != passengerId && !isStaff)
                return Forbid();

            // Numele complet — Passenger e inclus in GetWithDetailsAsync
            var fullName = reservation.Passenger != null
                ? $"{reservation.Passenger.FirstName} {reservation.Passenger.LastName}"
                : "—";

            var vm = new ReservationDetailsViewModel
            {
                ReservationId = reservation.ReservationId,
                AirlineName = reservation.Flight?.AirlineName ?? "—",
                Source = reservation.Flight?.Source ?? "—",
                Destination = reservation.Flight?.Destination ?? "—",
                DepartureTime = reservation.Flight?.DepartureTime ?? default,
                ArrivalTime = reservation.Flight?.ArrivalTime ?? default,
                SeatNumber = reservation.Seat?.SeatNumber ?? "—",
                PassengerFullName = fullName,
                TotalPrice = reservation.TotalPrice,
                ReservationTimeStamp = reservation.ReservationTimeStamp,
                Status = reservation.Status,
                CanCancel = reservation.Status != ReservationStatus.Cancelled
                         && (reservation.Flight?.DepartureTime ?? DateTime.MinValue)
                                > DateTime.UtcNow.AddHours(CANCELLATION_WINDOW_HOURS),
                CanPay = reservation.Status == ReservationStatus.Pending
            };

            return View(vm);
        }

        // ─────────────────────────────────────────────────────────────────────
        //  POST /Reservations/Cancel/5
        //  Anulare rezervare (REQ-40, REQ-41, BR-2)
        // ─────────────────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var passengerId = await GetCurrentPassengerIdAsync();
            if (passengerId == null) return Forbid();

            try
            {
                await _reservationService.CancelAsync(id, passengerId.Value);
                TempData["Success"] = "The reservation has been cancelled.";
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }

        // ─────────────────────────────────────────────────────────────────────
        //  GET /Reservations/Manifest/5
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet]
        [Authorize(Roles = "Administrator,Staff")]
        public async Task<IActionResult> Manifest(int flightId)
        {
            if (flightId <= 0) return BadRequest();

            var flight = await _uow.FlightRepository.GetByIdAsync(flightId);
            if (flight == null) return NotFound();

            var reservations = await _reservationService.GetManifestAsync(flightId);

            var vm = new ManifestViewModel
            {
                FlightId = flight.FlightId,
                AirlineName = flight.AirlineName,
                Source = flight.Source,
                Destination = flight.Destination,
                DepartureTime = flight.DepartureTime,
                Passengers = reservations
                    .Select(r => new ManifestRowViewModel
                    {
                        PassengerFullName = r.Passenger != null
                            ? $"{r.Passenger.FirstName} {r.Passenger.LastName}"
                            : "—",
                        SeatNumber = r.Seat?.SeatNumber ?? "—",
                        ReservationTimeStamp = r.ReservationTimeStamp
                    })
                    .OrderBy(p => p.SeatNumber)
                    .ToList()
            };

            return View(vm);
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Helper-e private
        // ─────────────────────────────────────────────────────────────────────
        private async Task<int?> GetCurrentPassengerIdAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return null;

            var profile = await _uow.PassengerProfileRepository.GetByUserIdAsync(userId);
            return profile?.PassengerId;
        }

        private async Task RehydrateCreateViewModelAsync(CreateReservationViewModel vm)
        {
            var flight = await _uow.FlightRepository.GetWithDetailsAsync(vm.FlightId);
            if (flight == null) return;

            vm.AirlineName = flight.AirlineName;
            vm.Source = flight.Source;
            vm.Destination = flight.Destination;
            vm.DepartureTime = flight.DepartureTime;
            vm.ArrivalTime = flight.ArrivalTime;

            // Consistenta cu Search: aplicam markup-ul pe afisare
            var markup = await _markupService.GetPlatformMarkupAsync();

            vm.AvailableClasses = flight.FlightClasses
                .Select(fc => new SelectListItem
                {
                    Value = fc.FlightClassId.ToString(),
                    Text = $"{fc.ClassName} — {(fc.Price + markup):C}",
                    Selected = fc.FlightClassId == vm.FlightClassId
                })
                .ToList();

            // MODIFICARE AICI: La fel, am scos .Where(s => s.IsAvailable)
            vm.AvailableSeats = flight.FlightSeats
                .Select(s => new AvailableSeatViewModel
                {
                    FlightSeatId = s.FlightSeatId,
                    SeatNumber = s.SeatNumber,
                    FlightClassId = s.FlightClassId,
                    IsAvailable = s.IsAvailable, // Trimitem statusul
                    ClassName = flight.FlightClasses
                        .FirstOrDefault(fc => fc.FlightClassId == s.FlightClassId)
                        ?.ClassName ?? "—"
                })
                .OrderBy(s => s.FlightSeatId)
                .ToList();
        }
    }
}