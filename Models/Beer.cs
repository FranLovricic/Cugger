namespace Cugger.Models
{
    /// <summary>
    /// Klasa koja predstavlja pivo
    /// </summary>
    public class Beer
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public BeerStyle Style { get; set; }
        public double ABV { get; set; } // Alcohol By Volume
        public int IBU { get; set; } // International Bitterness Units
        public string Description { get; set; } = string.Empty;
        public int BreweryId { get; set; }

        // Calculated properties
        public int RatingCount { get; set; }
        public double AverageRating { get; set; }

        // Relacije
        public virtual Brewery? Brewery { get; set; }
        public virtual List<CheckIn> CheckIns { get; set; } = new List<CheckIn>();
        public virtual List<Review> Reviews { get; set; } = new List<Review>();
    }
}
