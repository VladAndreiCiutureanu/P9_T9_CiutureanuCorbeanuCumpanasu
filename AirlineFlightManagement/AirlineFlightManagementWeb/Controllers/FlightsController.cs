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
        public async Task<IActionResult> Index(string source, string destination, DateTime? departureDate)
        {
            // Dacă utilizatorul doar a intrat pe pagină (fără a face un request de căutare), setăm variabile goale/default
            if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(destination) || !departureDate.HasValue)
            {
                ViewBag.Source = string.Empty;
                ViewBag.Destination = string.Empty;
                ViewBag.DepartureDate = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");

                // Putem afișa un mesaj doar dacă a încercat să caute ceva dar a omis câmpuri
                if (Request.Query.ContainsKey("source")) 
                {
                    ViewBag.ErrorMessage = "Toate câmpurile sunt obligatorii pentru căutare.";
                }

                // Returnăm view-ul cu o listă goală (niciun zbor)
                return View(new List<AirlineFlightManagement.Models.Models.Flight>());
            }

            var searchDate = departureDate.Value;
            var flights = await _flightService.GetAvailableFlightsAsync(source, destination, searchDate);

            // Trimitem datele înapoi la View
            ViewBag.Source = source;
            ViewBag.Destination = destination;
            ViewBag.DepartureDate = searchDate.ToString("yyyy-MM-dd");

            return View(flights);
        }
    }
}
