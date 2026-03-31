namespace Cugger.Models
{
    /// <summary>
    /// Klasa koja predstavlja objekt korisnika
    /// </summary>
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; }
        public string Bio { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;

        // Relacje
        public virtual List<CheckIn> CheckIns { get; set; } = new List<CheckIn>();
        public virtual List<Review> Reviews { get; set; } = new List<Review>();
        
        // N-N relacija za prijatelje
        public virtual List<Friendship> FromFriendships { get; set; } = new List<Friendship>();
        public virtual List<Friendship> ToFriendships { get; set; } = new List<Friendship>();
    }
}
