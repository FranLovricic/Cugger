using System.ComponentModel.DataAnnotations;

namespace Cugger.Models.ViewModels
{
    public class CreateCheckInViewModel
    {
        [Required(ErrorMessage = "Odaberi pivo.")]
        [Display(Name = "Pivo")]
        public int BeerId { get; set; }

        [Required(ErrorMessage = "Odaberi lokal.")]
        [Display(Name = "Lokal")]
        public int VenueId { get; set; }

        [Required]
        [Range(0.5, 5.0, ErrorMessage = "Ocjena mora biti između 0.5 i 5.0.")]
        [Display(Name = "Ocjena")]
        public double Rating { get; set; } = 4.0;

        [StringLength(1000)]
        [Display(Name = "Komentar")]
        public string Comment { get; set; } = string.Empty;

        [Display(Name = "Datum konzumiranja")]
        [DataType(DataType.Date)]
        public DateTime CheckInDate { get; set; } = DateTime.UtcNow.Date;

        // Pre-selected beer (kad korisnik klikne "+ Check-in" sa Beer/Details stranice)
        public int? PrefilledBeerId { get; set; }
    }
}
