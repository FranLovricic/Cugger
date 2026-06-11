using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Cugger.Models
{
    /// <summary>
    /// Aplikacijski korisnik (lab-5): ASP.NET Core Identity korisnik (int ključ)
    /// proširen domenskim poljima Cugger aplikacije.
    /// Naslijeđena Identity polja: UserName, Email, EmailConfirmed, PasswordHash...
    /// </summary>
    public class AppUser : IdentityUser<int>
    {
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

        /// <summary>
        /// Kompatibilnosni alias za starije dijelove aplikacije (lab 1-4)
        /// koji koriste "Username" umjesto Identity konvencije "UserName".
        /// </summary>
        [NotMapped]
        public string Username
        {
            get => UserName ?? string.Empty;
            set => UserName = value;
        }

        public virtual ICollection<CheckIn> CheckIns { get; set; } = new List<CheckIn>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

        public virtual ICollection<Friendship> FromFriendships { get; set; } = new List<Friendship>();
        public virtual ICollection<Friendship> ToFriendships { get; set; } = new List<Friendship>();
    }
}
