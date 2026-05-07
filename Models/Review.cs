using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cugger.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(User))]
        public int UserId { get; set; }

        [ForeignKey(nameof(Beer))]
        public int BeerId { get; set; }

        [Range(0, 5)]
        [Column(TypeName = "decimal(3,2)")]
        public double Rating { get; set; }

        [StringLength(2000)]
        public string Comment { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public int Likes { get; set; }

        public virtual User? User { get; set; }
        public virtual Beer? Beer { get; set; }
    }
}
