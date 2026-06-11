namespace Cugger.Models.Dto
{
    /// <summary>
    /// Lab-5: centralno mapiranje entitet → DTO (extension metode).
    /// </summary>
    public static class DtoMapper
    {
        // ---------- Brief ----------

        public static BreweryBriefDto ToBriefDto(this Brewery b) => new()
        {
            Id = b.Id,
            Name = b.Name,
            Country = b.Country,
            City = b.City
        };

        public static BeerBriefDto ToBriefDto(this Beer b) => new()
        {
            Id = b.Id,
            Name = b.Name,
            Style = b.Style.ToString(),
            Abv = b.ABV,
            Brewery = b.Brewery?.ToBriefDto()
        };

        public static UserBriefDto ToBriefDto(this AppUser u) => new()
        {
            Id = u.Id,
            Username = u.UserName ?? string.Empty,
            FirstName = u.FirstName,
            LastName = u.LastName,
            AvatarUrl = u.AvatarUrl
        };

        public static VenueBriefDto ToBriefDto(this Venue v) => new()
        {
            Id = v.Id,
            Name = v.Name,
            City = v.City,
            Country = v.Country
        };

        // ---------- Full ----------

        public static BeerDto ToDto(this Beer b, int ratingCount = 0, double averageRating = 0) => new()
        {
            Id = b.Id,
            Name = b.Name,
            Style = b.Style.ToString(),
            Abv = b.ABV,
            Ibu = b.IBU,
            Description = b.Description,
            ImageUrl = b.ImageUrl,
            Brewery = b.Brewery?.ToBriefDto(),
            RatingCount = ratingCount,
            AverageRating = Math.Round(averageRating, 2)
        };

        public static BreweryDto ToDto(this Brewery b, int beerCount = 0, bool includeBeers = false) => new()
        {
            Id = b.Id,
            Name = b.Name,
            Country = b.Country,
            City = b.City,
            FoundedYear = b.FoundedYear,
            Description = b.Description,
            WebsiteUrl = b.WebsiteUrl,
            LogoUrl = b.LogoUrl,
            BeerCount = beerCount,
            Beers = includeBeers ? b.Beers.Select(beer => beer.ToBriefDto()).ToList() : null
        };

        public static VenueDto ToDto(this Venue v, int checkInCount = 0) => new()
        {
            Id = v.Id,
            Name = v.Name,
            Address = v.Address,
            City = v.City,
            Country = v.Country,
            Latitude = v.Latitude,
            Longitude = v.Longitude,
            CheckInCount = checkInCount
        };

        public static UserDto ToDto(this AppUser u, int checkInCount = 0, int reviewCount = 0, int friendCount = 0) => new()
        {
            Id = u.Id,
            Username = u.UserName ?? string.Empty,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Bio = u.Bio,
            AvatarUrl = u.AvatarUrl,
            RegistrationDate = u.RegistrationDate,
            CheckInCount = checkInCount,
            ReviewCount = reviewCount,
            FriendCount = friendCount
        };

        public static CheckInDto ToDto(this CheckIn c) => new()
        {
            Id = c.Id,
            Rating = c.Rating,
            Comment = c.Comment,
            CheckInDate = c.CheckInDate,
            CreatedAt = c.CreatedAt,
            User = c.User?.ToBriefDto(),
            Beer = c.Beer?.ToBriefDto(),
            Venue = c.Venue?.ToBriefDto()
        };

        public static ReviewDto ToDto(this Review r) => new()
        {
            Id = r.Id,
            Rating = r.Rating,
            Comment = r.Comment,
            CreatedAt = r.CreatedAt,
            Likes = r.Likes,
            User = r.User?.ToBriefDto(),
            Beer = r.Beer?.ToBriefDto()
        };

        public static FriendshipDto ToDto(this Friendship f) => new()
        {
            Id = f.Id,
            CreatedAt = f.CreatedAt,
            FromUser = f.FromUser?.ToBriefDto(),
            ToUser = f.ToUser?.ToBriefDto()
        };

        public static BeerPhotoDto ToDto(this BeerPhoto p) => new()
        {
            Id = p.Id,
            BeerId = p.BeerId,
            FileName = p.FileName,
            ContentType = p.ContentType,
            SizeBytes = p.SizeBytes,
            Url = "/" + p.RelativePath.Replace('\\', '/'),
            UploadedAt = p.UploadedAt,
            UploadedBy = p.UploadedBy?.ToBriefDto()
        };
    }
}
