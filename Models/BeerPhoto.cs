using System.ComponentModel.DataAnnotations;

namespace Cugger.Models
{
    /// <summary>
    /// Datoteka (fotografija/dokument) uploadana uz konkretno pivo (lab-5).
    /// Sama datoteka se sprema na disk (wwwroot/uploads/beers/...),
    /// a u bazi se čuvaju metapodaci i relativna putanja.
    /// </summary>
    public class BeerPhoto
    {
        [Key]
        public int Id { get; set; }

        public int BeerId { get; set; }

        /// <summary>Originalni naziv datoteke kako ju je korisnik uploadao.</summary>
        [Required]
        [StringLength(260)]
        public string FileName { get; set; } = string.Empty;

        /// <summary>Generirani naziv pod kojim je datoteka spremljena na disk.</summary>
        [Required]
        [StringLength(260)]
        public string StoredFileName { get; set; } = string.Empty;

        [Required]
        [StringLength(120)]
        public string ContentType { get; set; } = string.Empty;

        public long SizeBytes { get; set; }

        /// <summary>Relativna putanja unutar wwwroot (npr. uploads/beers/3/abc.jpg).</summary>
        [Required]
        [StringLength(500)]
        public string RelativePath { get; set; } = string.Empty;

        public DateTime UploadedAt { get; set; }

        /// <summary>Tko je uploadao (null za anonimne/sistemske zapise).</summary>
        public int? UploadedByUserId { get; set; }

        public virtual Beer? Beer { get; set; }
        public virtual AppUser? UploadedBy { get; set; }
    }
}
