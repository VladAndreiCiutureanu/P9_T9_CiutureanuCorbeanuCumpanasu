using System.ComponentModel.DataAnnotations;

namespace AirlineFlightManagement.Models.ViewModels
{
    public class ProfileEditViewModel
    {
        [Required(ErrorMessage = "Prenumele este obligatoriu.")]
        [StringLength(50, ErrorMessage = "Prenumele nu poate depăși 50 de caractere.")]
        [Display(Name = "Prenume")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Numele de familie este obligatoriu.")]
        [StringLength(50, ErrorMessage = "Numele de familie nu poate depăși 50 de caractere.")]
        [Display(Name = "Nume de Familie")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Numărul de telefon este obligatoriu.")]
        [Phone(ErrorMessage = "Vă rugăm să introduceți un număr de telefon valid.")]
        [StringLength(30, ErrorMessage = "Numărul de telefon nu poate depăși 30 de caractere.")]
        [Display(Name = "Număr de Telefon")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Adresa este obligatorie.")]
        [StringLength(250, ErrorMessage = "Adresa nu poate depăși 250 de caractere.")]
        [Display(Name = "Adresă de Domiciliu / Contact")]
        public string Address { get; set; } = string.Empty;
    }
}