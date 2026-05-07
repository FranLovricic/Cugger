using System.ComponentModel.DataAnnotations;

namespace Cugger.Models
{
    public class Brewery
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Country { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        public int FoundedYear { get; set; }

        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [StringLength(300)]
        public string WebsiteUrl { get; set; } = string.Empty;

        [StringLength(500)]
        public string LogoUrl { get; set; } = string.Empty;

        public virtual ICollection<Beer> Beers { get; set; } = new List<Beer>();
    }
}
