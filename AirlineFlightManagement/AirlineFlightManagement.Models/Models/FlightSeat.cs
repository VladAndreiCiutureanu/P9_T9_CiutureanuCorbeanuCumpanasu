using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AirlineFlightManagement.Models.Models
{
    public class FlightSeat
    {
        [Key]
        public int FlightSeatId { get; set; }

        public int FlightId { get; set; }

        [ForeignKey(nameof(FlightId))]
        public virtual Flight? Flight { get; set; }

        public int FlightClassId { get; set; }

        [ForeignKey(nameof(FlightClassId))]
        public virtual FlightClass? FlightClass { get; set; }

        [Required]
        [MaxLength(10)]
        public string SeatNumber { get; set; } = string.Empty;

        public bool IsAvailable { get; set; } = true;

        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
