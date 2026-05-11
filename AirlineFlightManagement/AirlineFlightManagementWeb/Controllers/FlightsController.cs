using AirlineFlightManagement.Services.Interfaces;
using AirlineFlightManagementWeb.Models.ViewModels.Flight;
using Microsoft.AspNetCore.Mvc;

namespace AirlineFlightManagementWeb.Controllers
{
    /// <summary>
    /// Pagina publica de cautare zboruri.
    /// NOTA: Aceasta este o varianta minimala pentru a debloca testarea
    /// fluxului de booking end-to-end. Coleg B va extinde cu actiunile de admin
    /// (modificare pret, anulare zbor, etc.).
    /// </summary>
    public class FlightsController : Controller
    {
        private readonly IFlightService _flightService;

        public FlightsController(IFlightService flightService)
        {
            _flightService = flightService;
        }

        // GET /Flights/Search — afiseaza formularul gol
        // POST /Flights/Search — proceseaza cautarea
        [HttpGet]
        public IActionResult Search()
        {
            return View(new FlightSearchPageViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Search(FlightSearchPageViewModel vm)
        {
            // Validare basic
            if (string.IsNullOrWhiteSpace(vm.Source) ||
                string.IsNullOrWhiteSpace(vm.Destination) ||
                !vm.DepartureDate.HasValue)
            {
                ModelState.AddModelError(string.Empty,
                    "Completați toate câmpurile: origine, destinație și dată.");
                return View(vm);
            }

            try
            {
                var results = await _flightService.SearchAsync(
                    vm.Source, vm.Destination, vm.DepartureDate.Value);

                vm.Results = results.ToList();
                vm.SearchPerformed = true;
                return View(vm);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty,
                    $"Eroare la căutare: {ex.Message}");
                return View(vm);
            }
        }
    }
}
