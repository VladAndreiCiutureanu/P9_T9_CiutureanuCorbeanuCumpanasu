using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AirlineFlightManagement.Models.Models
{
    public class FlightSeat
    {
        public int FlightSeatId { get; set; }

        // Foreign key to the Flight
        public int FlightId { get; set; }
        [ForeignKey("FlightId")]
        public virtual Flight Flight { get; set; }

        // Foreign Key to the Flight Class
        public int FlightClassId { get; set; }
        [ForeignKey("FlightClassId")]
        public virtual FlightClass FlightClass { get; set; }

        public string SeatNumber { get; set; }
        
        public bool IsAvailable { get; set; } = true;

        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

    }
}
