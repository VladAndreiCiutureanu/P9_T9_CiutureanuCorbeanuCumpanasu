using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace AirlineFlightManagement.Models.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Additional properties for the ApplicationUser
        public bool IsActive { get; set; } = true;
        // Navigation property for the related PassengerProfile
        public virtual PassengerProfile PassengerProfile { get; set; }
    }
}
