using System.ComponentModel.DataAnnotations;

namespace Cugger.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(60)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(80)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(80)]
        public string LastName { get; set; } = string.Empty;

        public DateTime RegistrationDate { get; set; }

        [StringLength(500)]
        public string Bio { get; set; } = string.Empty;

        [StringLength(500)]
        public string AvatarUrl { get; set; } = string.Empty;

        // ====== Auth ======
        [Required]
        [StringLength(256)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [StringLength(64)]
        public string PasswordSalt { get; set; } = string.Empty;

        [StringLength(128)]
        public string? PasswordResetToken { get; set; }

        public DateTime? PasswordResetTokenExpiresAt { get; set; }

        public bool IsEmailConfirmed { get; set; }

        [StringLength(128)]
        public string? EmailConfirmationToken { get; set; }

        public DateTime? EmailConfirmationTokenExpiresAt { get; set; }

        public virtual ICollection<CheckIn> CheckIns { get; set; } = new List<CheckIn>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

        public virtual ICollection<Friendship> FromFriendships { get; set; } = new List<Friendship>();
        public virtual ICollection<Friendship> ToFriendships { get; set; } = new List<Friendship>();
    }
}
