using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cugger.Models
{
    public class Friendship
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(FromUser))]
        public int FromUserId { get; set; }

        [ForeignKey(nameof(ToUser))]
        public int ToUserId { get; set; }

        public DateTime CreatedAt { get; set; }

        public virtual User? FromUser { get; set; }
        public virtual User? ToUser { get; set; }
    }
}
