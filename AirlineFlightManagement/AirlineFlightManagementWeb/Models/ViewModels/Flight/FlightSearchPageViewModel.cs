using System.ComponentModel.DataAnnotations;
using AirlineFlightManagement.Services.Contracts;

namespace AirlineFlightManagementWeb.Models.ViewModels.Flight
{
    /// <summary>
    /// Pagina /Flights/Search — form de cautare + rezultate.
    /// NOTA: Acest controller e o varianta minimala pentru a debloca testarea
    /// end-to-end. Coleg B il va extinde cu pagini admin (modificare pret,
    /// anulare zbor, etc.) si UX rafinat.
    /// </summary>
    public class FlightSearchPageViewModel
    {
        [Display(Name = "De la")]
        public string? Source { get; set; }

        [Display(Name = "Spre")]
        public string? Destination { get; set; }

        [Display(Name = "Data plecării")]
        [DataType(DataType.Date)]
        public DateTime? DepartureDate { get; set; }

        // Populat dupa submit
        public List<FlightSearchResult> Results { get; set; } = new();

        // True dupa primul submit (ca sa stim cand sa afisam "no results")
        public bool SearchPerformed { get; set; }
    }
}
