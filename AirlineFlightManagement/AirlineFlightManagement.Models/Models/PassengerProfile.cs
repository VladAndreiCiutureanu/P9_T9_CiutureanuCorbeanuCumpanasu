using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AirlineFlightManagement.Models.Models
{
    public class PassengerProfile
    {
        [Key]
        public int PassengerId { get; set; }

        // Foreign key to the ApplicationUser
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual ApplicationUser UserAccount { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }

        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
