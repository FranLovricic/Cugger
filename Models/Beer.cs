using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cugger.Models
{
    public class Beer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        public BeerStyle Style { get; set; }

        [Column(TypeName = "decimal(4,2)")]
        public double ABV { get; set; }

        public int IBU { get; set; }

        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [StringLength(500)]
        public string ImageUrl { get; set; } = string.Empty;

        [ForeignKey(nameof(Brewery))]
        public int BreweryId { get; set; }

        [NotMapped]
        public int RatingCount { get; set; }

        [NotMapped]
        public double AverageRating { get; set; }

        public virtual Brewery? Brewery { get; set; }
        public virtual ICollection<CheckIn> CheckIns { get; set; } = new List<CheckIn>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<BeerPhoto> Photos { get; set; } = new List<BeerPhoto>();
    }
}
