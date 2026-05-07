using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AirlineFlightManagement.Models.Enums;

namespace AirlineFlightManagement.Models.Models
{
    public class Reservation
    {
        [Key]
        public int ReservationId { get; set; }

        public int PassengerId { get; set; }

        [ForeignKey(nameof(PassengerId))]
        public virtual PassengerProfile? Passenger { get; set; }

        public int FlightSeatId { get; set; }

        [ForeignKey(nameof(FlightSeatId))]
        public virtual FlightSeat? Seat { get; set; }

        // Redundant FK kept in sync with Seat.FlightId so we can enforce
        // BR-4 (one active reservation per passenger per flight) at the DB level.
        public int FlightId { get; set; }

        [ForeignKey(nameof(FlightId))]
        public virtual Flight? Flight { get; set; }

        public DateTime ReservationTimeStamp { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(typeof(decimal), "0", "1000000")]
        public decimal TotalPrice { get; set; }

        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
