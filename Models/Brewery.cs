namespace Cugger.Models
{
    /// <summary>
    /// Klasa koja predstavlja pivovar
    /// </summary>
    public class Brewery
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public int FoundedYear { get; set; }
        public string Description { get; set; } = string.Empty;
        public string WebsiteUrl { get; set; } = string.Empty;

        // Relacije
        public virtual List<Beer> Beers { get; set; } = new List<Beer>();
    }
}
