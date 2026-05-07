using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AirlineFlightManagement.Models.Models
{
    public class Aircraft
    {
        [Key]
        public int AircraftId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ModelName { get; set; } = string.Empty;

        [Range(1, 1000)]
        public int MaxCapacity { get; set; }

        public virtual ICollection<Flight> Flights { get; set; } = new List<Flight>();
    }
}
