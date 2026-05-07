using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AirlineFlightManagement.Models.Enums;

namespace AirlineFlightManagement.Models.Models
{
    public class Flight
    {
        [Key]
        public int FlightId { get; set; }

        public int AircraftId { get; set; }

        [ForeignKey(nameof(AircraftId))]
        public virtual Aircraft? Aircraft { get; set; }

        [Required]
        [MaxLength(64)]
        public string ExternalApiId { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string AirlineName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Source { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Destination { get; set; } = string.Empty;

        public DateTime DepartureTime { get; set; }

        public DateTime ArrivalTime { get; set; }

        public FlightStatus Status { get; set; } = FlightStatus.Scheduled;

        public virtual ICollection<FlightClass> FlightClasses { get; set; } = new List<FlightClass>();
        public virtual ICollection<FlightSeat> FlightSeats { get; set; } = new List<FlightSeat>();
        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
