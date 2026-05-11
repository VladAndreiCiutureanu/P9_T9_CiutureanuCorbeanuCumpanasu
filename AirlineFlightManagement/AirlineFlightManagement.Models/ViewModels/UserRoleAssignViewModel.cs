using System.ComponentModel.DataAnnotations;

namespace AirlineFlightManagement.Models.ViewModels
{
    public class UserRoleAssignViewModel
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Display(Name = "Adresă de Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vă rugăm să selectați un rol.")]
        [Display(Name = "Rol Atribuit")]
        public string SelectedRole { get; set; } = string.Empty;
    }
}