namespace Cugger.Models
{
    /// <summary>
    /// Klasa koja predstavlja mjesto/lokal (na kojem se obavlja check-in)
    /// </summary>
    public class Venue
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        // Relacije
        public virtual List<CheckIn> CheckIns { get; set; } = new List<CheckIn>();
    }
}
