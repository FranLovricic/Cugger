namespace Cugger.Models
{
    /// <summary>
    /// Klasa koja predstavlja recenziju piva
    /// </summary>
    public class Review
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int BeerId { get; set; }
        public double Rating { get; set; } // 0-5
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int Likes { get; set; }

        // Relacije
        public virtual User? User { get; set; }
        public virtual Beer? Beer { get; set; }
    }
}
