using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AirlineFlightManagement.Models.Models;

namespace AirlineFlightManagement.Services.Interfaces
{
    public interface ISerpApiClient
    {
        // metoda care cauta zboruri folosind SerpApi
        Task<IEnumerable<Flight>> SearchFlightsAsync(string source, string destination, DateTime departureDate);
    }
}
