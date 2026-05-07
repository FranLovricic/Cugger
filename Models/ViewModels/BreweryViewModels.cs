using System.ComponentModel.DataAnnotations;

namespace Cugger.Models.ViewModels
{
    public class CreateBreweryViewModel
    {
        [Required(ErrorMessage = "Naziv pivovare je obavezan.")]
        [StringLength(150, MinimumLength = 2)]
        [Display(Name = "Naziv pivovare")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Država je obavezna.")]
        [StringLength(100)]
        [Display(Name = "Država")]
        public string Country { get; set; } = string.Empty;

        [Required(ErrorMessage = "Grad je obavezan.")]
        [StringLength(100)]
        [Display(Name = "Grad")]
        public string City { get; set; } = string.Empty;

        [Range(1000, 2100, ErrorMessage = "Godina osnivanja mora biti između 1000 i 2100.")]
        [Display(Name = "Godina osnivanja")]
        public int FoundedYear { get; set; } = DateTime.UtcNow.Year;

        [StringLength(2000)]
        [Display(Name = "Opis")]
        public string Description { get; set; } = string.Empty;

        [StringLength(300)]
        [Url(ErrorMessage = "Neispravan URL.")]
        [Display(Name = "Web stranica")]
        public string? WebsiteUrl { get; set; }
    }
}
