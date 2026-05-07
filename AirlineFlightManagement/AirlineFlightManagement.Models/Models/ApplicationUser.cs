using Microsoft.AspNetCore.Identity;

namespace AirlineFlightManagement.Models.Models
{
    public class ApplicationUser : IdentityUser
    {
        public bool IsActive { get; set; } = true;

        public virtual PassengerProfile? PassengerProfile { get; set; }
    }
}
