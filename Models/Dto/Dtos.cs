namespace Cugger.Models.Dto
{
    // ============================================================
    // Lab-5: DTO klase koje API vraća klijentu.
    // Interna polja entiteta (password hash, security stamp, email,
    // concurrency stampovi...) se NE izlažu kroz API.
    // Povezani podaci se vraćaju kroz ugniježđene "brief" DTO klase.
    // ============================================================

    // ---------- Brief (ugniježđeni) DTO-i ----------

    public class BreweryBriefDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
    }

    public class BeerBriefDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Style { get; set; } = string.Empty;
        public double Abv { get; set; }
        public BreweryBriefDto? Brewery { get; set; }
    }

    public class UserBriefDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
    }

    public class VenueBriefDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
    }

    // ---------- Glavni (read) DTO-i ----------

    public class BeerDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Style { get; set; } = string.Empty;
        public double Abv { get; set; }
        public int Ibu { get; set; }
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public BreweryBriefDto? Brewery { get; set; }
        public int RatingCount { get; set; }
        public double AverageRating { get; set; }
    }

    public class BreweryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public int FoundedYear { get; set; }
        public string Description { get; set; } = string.Empty;
        public string WebsiteUrl { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;
        public int BeerCount { get; set; }
        /// <summary>Popunjeno samo na detail endpointu (GET api/breweries/{id}).</summary>
        public List<BeerBriefDto>? Beers { get; set; }
    }

    public class VenueDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int CheckInCount { get; set; }
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; }
        public int CheckInCount { get; set; }
        public int ReviewCount { get; set; }
        public int FriendCount { get; set; }
    }

    public class CheckInDto
    {
        public int Id { get; set; }
        public double Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CheckInDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public UserBriefDto? User { get; set; }
        public BeerBriefDto? Beer { get; set; }
        public VenueBriefDto? Venue { get; set; }
    }

    public class ReviewDto
    {
        public int Id { get; set; }
        public double Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int Likes { get; set; }
        public UserBriefDto? User { get; set; }
        public BeerBriefDto? Beer { get; set; }
    }

    public class FriendshipDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public UserBriefDto? FromUser { get; set; }
        public UserBriefDto? ToUser { get; set; }
    }

    public class BeerPhotoDto
    {
        public int Id { get; set; }
        public int BeerId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long SizeBytes { get; set; }
        /// <summary>Javni URL datoteke (relativno na root aplikacije).</summary>
        public string Url { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
        public UserBriefDto? UploadedBy { get; set; }
    }
}
