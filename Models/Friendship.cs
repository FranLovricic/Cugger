namespace Cugger.Models
{
    /// <summary>
    /// Klasa koja predstavlja prijateljstvo između dva korisnika (N-N relacija)
    /// </summary>
    public class Friendship
    {
        public int Id { get; set; }
        public int FromUserId { get; set; }
        public int ToUserId { get; set; }
        public DateTime CreatedAt { get; set; }

        // Relacije
        public virtual User? FromUser { get; set; }
        public virtual User? ToUser { get; set; }
    }
}
