using System.ComponentModel.DataAnnotations;

namespace Cugger.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Korisničko ime je obavezno.")]
        [StringLength(60, MinimumLength = 3, ErrorMessage = "Username mora imati 3-60 znakova.")]
        [RegularExpression("^[a-zA-Z0-9_.-]+$", ErrorMessage = "Username smije sadržavati slova, brojeve, '_', '.', '-'.")]
        [Display(Name = "Korisničko ime")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email je obavezan.")]
        [EmailAddress(ErrorMessage = "Neispravan email.")]
        [StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ime je obavezno.")]
        [StringLength(80)]
        [Display(Name = "Ime")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Prezime je obavezno.")]
        [StringLength(80)]
        [Display(Name = "Prezime")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lozinka je obavezna.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Lozinka mora imati najmanje 8 znakova.")]
        [DataType(DataType.Password)]
        [Display(Name = "Lozinka")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Potvrda lozinke je obavezna.")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Lozinke se ne podudaraju.")]
        [Display(Name = "Potvrdi lozinku")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "Unesi korisničko ime ili email.")]
        [Display(Name = "Korisničko ime ili email")]
        public string UsernameOrEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Unesi lozinku.")]
        [DataType(DataType.Password)]
        [Display(Name = "Lozinka")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Zapamti me")]
        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Email je obavezan.")]
        [EmailAddress(ErrorMessage = "Neispravan email.")]
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordViewModel
    {
        [Required]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email je obavezan.")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lozinka je obavezna.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Lozinka mora imati najmanje 8 znakova.")]
        [DataType(DataType.Password)]
        [Display(Name = "Nova lozinka")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Lozinke se ne podudaraju.")]
        [Display(Name = "Potvrdi lozinku")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
