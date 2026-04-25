using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AirlineFlightManagement.Models.Models
{
    public class Flight
    {
        public int FlightId { get; set; }

        // Foreign Key to the Aircraft
        public int AircraftId { get; set; }
        [ForeignKey("AircraftId")]
        public virtual Aircraft Aircraft { get; set; }

        public string ExternalApiId { get; set; } // ID from external API

        public string AirlineName { get; set; }

        public string Source { get; set; }
        public string Destination { get; set; }

        public DateTime DepartureTime { get; set; }

        public DateTime ArrivalTime { get; set; }

        public string Status { get; set; } // e.g., Scheduled, Delayed, Cancelled

        // Navigation properties for related entities
        public virtual ICollection<FlightClass> FlightClasses { get; set; } = new List<FlightClass>();
        public virtual ICollection<FlightSeat> FlightSeats { get; set; } = new List<FlightSeat>();
    }
}
