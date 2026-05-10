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

            if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(destination) || !departureDate.HasValue)
            {
                ViewBag.Source = string.Empty;
                ViewBag.Destination = string.Empty;
                ViewBag.DepartureDate = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");

                if (Request.Query.ContainsKey("source")) 
                {
                    ViewBag.ErrorMessage = "All search fields are required.";
                }

                // return empty view with error message if parameters are missing
                return View(new List<AirlineFlightManagement.Models.Models.Flight>());
            }

            var searchDate = departureDate.Value;
            var flights = await _flightService.GetAvailableFlightsAsync(source, destination, searchDate);

            // we get the codes for the source and destination to pass to the View
            var safeSource = AirlineFlightManagement.Services.Helpers.LocationMapper.GetSafeCode(source);
            var safeDest = AirlineFlightManagement.Services.Helpers.LocationMapper.GetSafeCode(destination);

            // send back the original input values to the View so they can be displayed in the search form
            ViewBag.Source = source; 
            ViewBag.Destination = destination;
            ViewBag.SafeSourceCode = safeSource;
            ViewBag.SafeDestCode = safeDest;
            ViewBag.DepartureDate = searchDate.ToString("yyyy-MM-dd");

            // if error
            var flightsList = flights.ToList();
            if (flightsList.Count == 1 && flightsList.First().Source == "EROARE")
            {
                ViewBag.ErrorMessage = "Error, no flights found. Please check the search criteria and try again!";
                flightsList.Clear();
            }

            return View(flightsList);
        }
    }
}
