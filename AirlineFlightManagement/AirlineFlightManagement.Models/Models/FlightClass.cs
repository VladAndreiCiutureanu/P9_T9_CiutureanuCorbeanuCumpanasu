using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AirlineFlightManagement.Models.Models
{
    public class FlightClass
    {
        public int FlightClassId { get; set; }

        // Foreign key to the Flight
        public int FlightId { get; set; }
        [ForeignKey("FlightId")]
        public virtual Flight Flight { get; set; }

        public string ClassName { get; set; } // e.g., Economy, Business, First

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        // Navigation property for the related FlightSeats
        public virtual ICollection<FlightSeat> FlightSeats { get; set; } = new List<FlightSeat>();

    }
}
