using System.ComponentModel.DataAnnotations;

namespace Cugger.Models.ViewModels
{
    public class CreateBeerViewModel
    {
        [Required(ErrorMessage = "Naziv piva je obavezan.")]
        [StringLength(150, MinimumLength = 2)]
        [Display(Name = "Naziv piva")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Pivovara je obavezna.")]
        [Display(Name = "Pivovara")]
        public int BreweryId { get; set; }

        [Required]
        [Display(Name = "Stil")]
        public BeerStyle Style { get; set; } = BeerStyle.Lager;

        [Required(ErrorMessage = "ABV je obavezan.")]
        [Range(0.0, 25.0, ErrorMessage = "ABV mora biti između 0% i 25%.")]
        [Display(Name = "ABV (%)")]
        public double ABV { get; set; }

        [Required]
        [Range(0, 200, ErrorMessage = "IBU mora biti između 0 i 200.")]
        [Display(Name = "IBU")]
        public int IBU { get; set; }

        [StringLength(2000)]
        [Display(Name = "Opis")]
        public string Description { get; set; } = string.Empty;

        [StringLength(500)]
        [Url(ErrorMessage = "Neispravan URL.")]
        [Display(Name = "Slika piva (URL)")]
        public string? ImageUrl { get; set; }
    }
}
