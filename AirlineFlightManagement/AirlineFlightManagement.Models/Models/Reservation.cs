using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AirlineFlightManagement.Models.Models
{
    public class Reservation
    {
        public int ReservationId { get; set; }

        // Foreign key to the PassengerProfile
        public int PassengerId { get; set; }
        [ForeignKey("PassengerId")]
        public virtual PassengerProfile Passenger { get; set; }

        // Foreign key to the FlightSeat
        public int FlightSeatId { get; set; }
        [ForeignKey("FlightSeatId")]
        public virtual FlightSeat Seat { get; set; }

        public DateTime ReservationTimeStamp { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public string Status { get; set; }

        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
