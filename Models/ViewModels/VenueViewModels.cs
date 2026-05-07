using System.ComponentModel.DataAnnotations;

namespace Cugger.Models.ViewModels
{
    public class CreateVenueViewModel
    {
        [Required(ErrorMessage = "Naziv lokala je obavezan.")]
        [StringLength(150, MinimumLength = 2)]
        [Display(Name = "Naziv lokala")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Adresa je obavezna.")]
        [StringLength(250)]
        [Display(Name = "Adresa")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Grad je obavezan.")]
        [StringLength(100)]
        [Display(Name = "Grad")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Država je obavezna.")]
        [StringLength(100)]
        [Display(Name = "Država")]
        public string Country { get; set; } = string.Empty;

        [Range(-90.0, 90.0)]
        [Display(Name = "Geografska širina")]
        public double Latitude { get; set; }

        [Range(-180.0, 180.0)]
        [Display(Name = "Geografska dužina")]
        public double Longitude { get; set; }
    }
}
