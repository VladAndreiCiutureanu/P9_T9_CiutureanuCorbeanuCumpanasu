using System.ComponentModel.DataAnnotations;

namespace AirlineFlightManagement.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Adresa de email este obligatorie.")]
        [EmailAddress(ErrorMessage = "Vă rugăm să introduceți o adresă de email validă.")]
        [Display(Name = "Adresă de Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Parola este obligatorie.")]
        [DataType(DataType.Password)]
        [Display(Name = "Parolă")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Ține-mă minte")]
        public bool RememberMe { get; set; }
    }
}