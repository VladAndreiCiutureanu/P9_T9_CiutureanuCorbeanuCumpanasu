using System;
using System.Collections.Generic;
using System.Text;

namespace AirlineFlightManagement.Models.Models
{
    public class Aircraft
    {
        public int AircraftId { get; set; }
        public string ModelName { get; set; }
        public int MaxCapacity { get; set; }

        // Navigation property for related Flights
        public virtual ICollection<Flight> Flights { get; set; } = new List<Flight>();
    }
}
