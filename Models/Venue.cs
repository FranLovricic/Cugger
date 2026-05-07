using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cugger.Models
{
    public class Venue
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(250)]
        public string Address { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Country { get; set; } = string.Empty;

        [Column(TypeName = "decimal(9,6)")]
        public double Latitude { get; set; }

        [Column(TypeName = "decimal(9,6)")]
        public double Longitude { get; set; }

        public virtual ICollection<CheckIn> CheckIns { get; set; } = new List<CheckIn>();
    }
}
