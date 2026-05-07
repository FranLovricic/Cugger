using System.ComponentModel.DataAnnotations;

namespace Cugger.Models.ViewModels
{
    public class CreateReviewViewModel
    {
        [Required(ErrorMessage = "Odaberi pivo.")]
        [Display(Name = "Pivo")]
        public int BeerId { get; set; }

        [Required]
        [Range(0.5, 5.0, ErrorMessage = "Ocjena mora biti između 0.5 i 5.0.")]
        [Display(Name = "Ocjena")]
        public double Rating { get; set; } = 4.0;

        [Required(ErrorMessage = "Recenzija ne može biti prazna.")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Recenzija mora imati 10-2000 znakova.")]
        [Display(Name = "Recenzija")]
        public string Comment { get; set; } = string.Empty;
    }
}
