using System.ComponentModel.DataAnnotations;

namespace AirlineFlightManagement.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Adresa de email este obligatorie.")]
        [EmailAddress(ErrorMessage = "Vă rugăm să introduceți o adresă de email validă.")]
        [Display(Name = "Adresă de Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Parola este obligatorie.")]
        [DataType(DataType.Password)]
        [Display(Name = "Parolă")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirmarea parolei este obligatorie.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmare Parolă")]
        [Compare(nameof(Password), ErrorMessage = "Parola și confirmarea parolei nu corespund.")]
        public string ConfirmPassword { get; set; } = string.Empty;

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