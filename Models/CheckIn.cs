using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cugger.Models
{
    public class CheckIn
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(User))]
        public int UserId { get; set; }

        [ForeignKey(nameof(Beer))]
        public int BeerId { get; set; }

        [ForeignKey(nameof(Venue))]
        public int VenueId { get; set; }

        [Range(0, 5)]
        [Column(TypeName = "decimal(3,2)")]
        public double Rating { get; set; }

        [StringLength(1000)]
        public string Comment { get; set; } = string.Empty;

        public DateTime CheckInDate { get; set; }

        public DateTime CreatedAt { get; set; }

        public virtual AppUser? User { get; set; }
        public virtual Beer? Beer { get; set; }
        public virtual Venue? Venue { get; set; }
    }
}
