using AirlineFlightManagement.Services.Interfaces;
using AirlineFlightManagementWeb.Models.ViewModels.Report;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirlineFlightManagementWeb.Controllers
{
    /// <summary>
    /// Controller pentru rapoarte (Coleg C).
    ///
    /// Acces de baza: Administrator si Staff.
    /// Raportul de Revenue (REQ-44) e restrictionat la Administrator.
    /// </summary>
    [Authorize(Roles = "Administrator,Staff")]
    public class ReportsController : Controller
    {
        private readonly IReportService _reportService;

        // Default range: ultimele 30 de zile cand utilizatorul nu specifica alt interval.
        private const int DEFAULT_RANGE_DAYS = 30;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  GET /Reports
        //  Dashboard — pagina cu link-uri spre rapoartele disponibile.
        //  Nu are nevoie de ViewModel: e doar navigatie.
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  GET /Reports/Occupancy?from=...&to=...
        //  Raport ocupare pe interval (REQ-46)
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Occupancy(DateTime? from = null, DateTime? to = null)
        {
            // Default: ultimele 30 de zile incheiate cu ziua curenta.
            // Important: indiferent daca data vine din query string sau din default,
            //   - "from" = 00:00:00 (inceputul zilei)
            //   - "to"   = 23:59:59 (sfarsitul zilei) — altfel pierdem zborurile
            //              de dupa miezul noptii ale zilei to.
            var fromDate = (from ?? DateTime.UtcNow.Date.AddDays(-DEFAULT_RANGE_DAYS)).Date;
            var toDate = (to ?? DateTime.UtcNow.Date).Date.AddDays(1).AddSeconds(-1);

            // Validare interval
            if (fromDate > toDate)
            {
                ModelState.AddModelError(string.Empty,
                    "Data de inceput trebuie sa fie inainte de data de final.");
                return View(new OccupancyReportViewModel
                {
                    From = fromDate,
                    To = toDate
                });
            }

            var reports = await _reportService.GetOccupancyForRangeAsync(fromDate, toDate);

            var vm = new OccupancyReportViewModel
            {
                From = fromDate,
                To = toDate,
                Reports = reports.ToList()
            };

            return View(vm);
        }

        // ─────────────────────────────────────────────────────────────────────
        //  GET /Reports/FlightOccupancy/{flightId}
        //  Raport ocupare pentru un singur zbor (REQ-46)
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> FlightOccupancy(int flightId)
        {
            if (flightId <= 0) return BadRequest();

            try
            {
                var report = await _reportService.GetFlightOccupancyAsync(flightId);
                return View(new FlightOccupancyViewModel { Report = report });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        //  GET /Reports/Revenue?from=...&to=...
        //  Raport financiar (REQ-44) — DOAR Administrator
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Revenue(DateTime? from = null, DateTime? to = null)
        {
            // Vezi comentariul de la Occupancy — to se extinde la sfarsitul zilei.
            var fromDate = (from ?? DateTime.UtcNow.Date.AddDays(-DEFAULT_RANGE_DAYS)).Date;
            var toDate = (to ?? DateTime.UtcNow.Date).Date.AddDays(1).AddSeconds(-1);

            if (fromDate > toDate)
            {
                ModelState.AddModelError(string.Empty,
                    "Data de inceput trebuie sa fie inainte de data de final.");
                return View(new RevenueReportViewModel
                {
                    From = fromDate,
                    To = toDate
                });
            }

            var rows = await _reportService.GetRevenueAsync(fromDate, toDate);

            var vm = new RevenueReportViewModel
            {
                From = fromDate,
                To = toDate,
                Rows = rows.ToList()
            };

            return View(vm);
        }
    }
}
