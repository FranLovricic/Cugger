using System.ComponentModel.DataAnnotations;

namespace Cugger.Models.Dto
{
    // ============================================================
    // Lab-5: ulazni DTO-i za POST/PUT API operacije, s validacijom.
    // [ApiController] automatski vraća 400 + ValidationProblemDetails
    // kad validacija ne prođe.
    // ============================================================

    public class BeerInputDto
    {
        [Required(ErrorMessage = "Naziv piva je obavezan.")]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        /// <summary>Stil piva (string vrijednost BeerStyle enuma, npr. "IPA").</summary>
        [Required]
        [EnumDataType(typeof(BeerStyle), ErrorMessage = "Nepoznat stil piva.")]
        public string Style { get; set; } = string.Empty;

        [Range(0, 70, ErrorMessage = "ABV mora biti između 0 i 70.")]
        public double Abv { get; set; }

        [Range(0, 200, ErrorMessage = "IBU mora biti između 0 i 200.")]
        public int Ibu { get; set; }

        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [StringLength(500)]
        public string ImageUrl { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "BreweryId je obavezan.")]
        public int BreweryId { get; set; }
    }

    public class BreweryInputDto
    {
        [Required(ErrorMessage = "Naziv pivovare je obavezan.")]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Država je obavezna.")]
        [StringLength(100)]
        public string Country { get; set; } = string.Empty;

        [Required(ErrorMessage = "Grad je obavezan.")]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Range(1000, 2100, ErrorMessage = "Godina osnutka mora biti između 1000 i 2100.")]
        public int FoundedYear { get; set; }

        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [StringLength(300)]
        public string WebsiteUrl { get; set; } = string.Empty;

        [StringLength(500)]
        public string LogoUrl { get; set; } = string.Empty;
    }

    public class VenueInputDto
    {
        [Required(ErrorMessage = "Naziv lokala je obavezan.")]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Adresa je obavezna.")]
        [StringLength(250)]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Grad je obavezan.")]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Država je obavezna.")]
        [StringLength(100)]
        public string Country { get; set; } = string.Empty;

        [Range(-90, 90)]
        public double Latitude { get; set; }

        [Range(-180, 180)]
        public double Longitude { get; set; }
    }

    public class UserCreateDto
    {
        [Required(ErrorMessage = "Korisničko ime je obavezno.")]
        [StringLength(60, MinimumLength = 3)]
        [RegularExpression("^[a-zA-Z0-9_.-]+$", ErrorMessage = "Username smije sadržavati slova, brojeve, '_', '.', '-'.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email je obavezan.")]
        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lozinka je obavezna.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Lozinka mora imati najmanje 8 znakova.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ime je obavezno.")]
        [StringLength(80)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Prezime je obavezno.")]
        [StringLength(80)]
        public string LastName { get; set; } = string.Empty;

        [StringLength(500)]
        public string Bio { get; set; } = string.Empty;

        [StringLength(500)]
        public string AvatarUrl { get; set; } = string.Empty;
    }

    /// <summary>Izmjena profila — username/email/lozinka se ne mijenjaju kroz ovaj endpoint.</summary>
    public class UserUpdateDto
    {
        [Required(ErrorMessage = "Ime je obavezno.")]
        [StringLength(80)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Prezime je obavezno.")]
        [StringLength(80)]
        public string LastName { get; set; } = string.Empty;

        [StringLength(500)]
        public string Bio { get; set; } = string.Empty;

        [StringLength(500)]
        public string AvatarUrl { get; set; } = string.Empty;
    }

    public class CheckInInputDto
    {
        /// <summary>Ako nije zadan, koristi se trenutno prijavljeni korisnik. Samo Admin smije zadati tuđi id.</summary>
        public int? UserId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "BeerId je obavezan.")]
        public int BeerId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "VenueId je obavezan.")]
        public int VenueId { get; set; }

        [Range(0, 5, ErrorMessage = "Ocjena mora biti između 0 i 5.")]
        public double Rating { get; set; }

        [StringLength(1000)]
        public string Comment { get; set; } = string.Empty;

        public DateTime? CheckInDate { get; set; }
    }

    public class ReviewInputDto
    {
        /// <summary>Ako nije zadan, koristi se trenutno prijavljeni korisnik. Samo Admin smije zadati tuđi id.</summary>
        public int? UserId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "BeerId je obavezan.")]
        public int BeerId { get; set; }

        [Range(0, 5, ErrorMessage = "Ocjena mora biti između 0 i 5.")]
        public double Rating { get; set; }

        [Required(ErrorMessage = "Komentar recenzije je obavezan.")]
        [StringLength(2000)]
        public string Comment { get; set; } = string.Empty;
    }

    public class FriendshipInputDto
    {
        /// <summary>Ako nije zadan, koristi se trenutno prijavljeni korisnik. Samo Admin smije zadati tuđi id.</summary>
        public int? FromUserId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "ToUserId je obavezan.")]
        public int ToUserId { get; set; }
    }
}
