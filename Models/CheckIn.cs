namespace Cugger.Models
{
    /// <summary>
    /// Klasa koja predstavlja check-in (zapis o konzumaciji piva na lokaciji)
    /// </summary>
    public class CheckIn
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int BeerId { get; set; }
        public int VenueId { get; set; }
        public double Rating { get; set; } // 0-5
        public string Comment { get; set; } = string.Empty;
        public DateTime CheckInDate { get; set; }
        public DateTime CreatedAt { get; set; }

        // Relacije
        public virtual User? User { get; set; }
        public virtual Beer? Beer { get; set; }
        public virtual Venue? Venue { get; set; }
    }
}
