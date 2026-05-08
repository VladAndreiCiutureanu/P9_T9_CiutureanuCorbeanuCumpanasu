using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AirlineFlightManagement.Services.Interfaces;

namespace AirlineFlightManagementWeb.Controllers
{
    public class FlightsController : Controller
    {
        private readonly IFlightService _flightService;

        public FlightsController(IFlightService flightService)
        {
            _flightService = flightService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string source = "Bucuresti", string destination = "Londra")
        {
            // cautam zborur
            DateTime mockDate = DateTime.Now.AddDays(1);

            var flights = await _flightService.GetAvailableFlightsAsync(source, destination, mockDate);

            // trimitem datele reale mai departe catre View 
            ViewBag.Source = source;
            ViewBag.Destination = destination;

            return View(flights);
        }
    }
}
