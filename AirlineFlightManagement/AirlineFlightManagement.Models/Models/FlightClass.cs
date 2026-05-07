using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AirlineFlightManagement.Models.Models
{
    public class FlightClass
    {
        [Key]
        public int FlightClassId { get; set; }

        public int FlightId { get; set; }

        [ForeignKey(nameof(FlightId))]
        public virtual Flight? Flight { get; set; }

        [Required]
        [MaxLength(50)]
        public string ClassName { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        [Range(typeof(decimal), "0", "1000000")]
        public decimal Price { get; set; }

        public virtual ICollection<FlightSeat> FlightSeats { get; set; } = new List<FlightSeat>();
    }
}
