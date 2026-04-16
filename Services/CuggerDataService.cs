using Cugger.Models;

namespace Cugger.Services
{
    /// <summary>
    /// In-memory data service za Cugger - čuva sve podatke tijekom izvršavanja
    /// </summary>
    public class CuggerDataService
    {
        private static CuggerDataService? _instance;
        private readonly List<Brewery> _breweries;
        private readonly List<Beer> _beers;
        private readonly List<User> _users;
        private readonly List<CheckIn> _checkIns;
        private readonly List<Review> _reviews;
        private readonly List<Venue> _venues;
        private readonly List<Friendship> _friendships;

        private CuggerDataService()
        {
            // Inicijalizacija pivovara
            _breweries = new List<Brewery>
            {
                new Brewery
                {
                    Id = 1,
                    Name = "Karlovačka Pivovara",
                    Country = "Hrvatska",
                    City = "Karlovac",
                    FoundedYear = 1854,
                    Description = "Najstarija pivovara u Hrvatskoj",
                    WebsiteUrl = "https://www.karlovacka.hr"
                },
                new Brewery
                {
                    Id = 2,
                    Name = "Stone Brewing",
                    Country = "SAD",
                    City = "San Diego",
                    FoundedYear = 1996,
                    Description = "Poznata za svoje IPA piva",
                    WebsiteUrl = "https://www.stonebrewing.com"
                },
                new Brewery
                {
                    Id = 3,
                    Name = "Guinness Brewery",
                    Country = "Irska",
                    City = "Dublin",
                    FoundedYear = 1759,
                    Description = "Legendarni proizvodač Guinnessa",
                    WebsiteUrl = "https://www.guinness.com"
                }
            };

            // Inicijalizacija piva
            _beers = new List<Beer>
            {
                new Beer
                {
                    Id = 1,
                    Name = "Karlovačko",
                    BreweryId = 1,
                    Style = BeerStyle.Lager,
                    ABV = 5.1,
                    IBU = 20,
                    Description = "Klasično hrvatsko lager pivo",
                    RatingCount = 0,
                    AverageRating = 0
                },
                new Beer
                {
                    Id = 2,
                    Name = "Stone IPA",
                    BreweryId = 2,
                    Style = BeerStyle.IPA,
                    ABV = 6.9,
                    IBU = 77,
                    Description = "Aromatično IPA pivo s bogatom gorčinom",
                    RatingCount = 0,
                    AverageRating = 0
                },
                new Beer
                {
                    Id = 3,
                    Name = "Guinness Extra Stout",
                    BreweryId = 3,
                    Style = BeerStyle.Stout,
                    ABV = 4.3,
                    IBU = 45,
                    Description = "Klasični Guinness Stout sa karakterističnom tamnom bojom",
                    RatingCount = 0,
                    AverageRating = 0
                },
                new Beer
                {
                    Id = 4,
                    Name = "Stella Artois",
                    BreweryId = 1,
                    Style = BeerStyle.Pilsner,
                    ABV = 5.0,
                    IBU = 30,
                    Description = "Premium belgijsko pilsner pivo",
                    RatingCount = 0,
                    AverageRating = 0
                },
                new Beer
                {
                    Id = 5,
                    Name = "Stone Ruination",
                    BreweryId = 2,
                    Style = BeerStyle.IPA,
                    ABV = 7.7,
                    IBU = 100,
                    Description = "Ekstremno hopnog IPA s intenzivnom gorčinom",
                    RatingCount = 0,
                    AverageRating = 0
                }
            };

            // Inicijalizacija lokala
            _venues = new List<Venue>
            {
                new Venue
                {
                    Id = 1,
                    Name = "The Beer Garden",
                    Address = "Ulica 1, broj 10",
                    City = "Zagreb",
                    Country = "Hrvatska",
                    Latitude = 45.815,
                    Longitude = 15.982
                },
                new Venue
                {
                    Id = 2,
                    Name = "Craft Beer Pub",
                    Address = "Ulica 2, broj 20",
                    City = "Zagreb",
                    Country = "Hrvatska",
                    Latitude = 45.816,
                    Longitude = 15.985
                },
                new Venue
                {
                    Id = 3,
                    Name = "Irish Pub Dublin",
                    Address = "O'Connell Street, broj 1",
                    City = "Dublin",
                    Country = "Irska",
                    Latitude = 53.349,
                    Longitude = -6.260
                }
            };

            // Inicijalizacija korisnika
            _users = new List<User>
            {
                new User
                {
                    Id = 1,
                    Username = "pivo_lover",
                    Email = "dragan@example.com",
                    FirstName = "Dragan",
                    LastName = "Marić",
                    RegistrationDate = new DateTime(2023, 1, 15),
                    Bio = "Apsolventist pivarstva i ljubitelj kvalitetnih piva",
                    AvatarUrl = "https://ui-avatars.com/api/?name=Dragan+Maric&background=F59E0B&color=fff"
                },
                new User
                {
                    Id = 2,
                    Username = "hop_king",
                    Email = "marko@example.com",
                    FirstName = "Marko",
                    LastName = "Horvat",
                    RegistrationDate = new DateTime(2023, 3, 20),
                    Bio = "IPA entuzijast, traži nove craft pivaovare",
                    AvatarUrl = "https://ui-avatars.com/api/?name=Marko+Horvat&background=D97706&color=fff"
                },
                new User
                {
                    Id = 3,
                    Username = "stout_fan",
                    Email = "ana@example.com",
                    FirstName = "Ana",
                    LastName = "Novak",
                    RegistrationDate = new DateTime(2023, 6, 10),
                    Bio = "Ljubiteljica temnih piva i europskih pivovar",
                    AvatarUrl = "https://ui-avatars.com/api/?name=Ana+Novak&background=FCD34D&color=fff"
                }
            };

            // Inicijalizacija check-inova
            _checkIns = new List<CheckIn>
            {
                new CheckIn
                {
                    Id = 1,
                    UserId = 1,
                    BeerId = 1,
                    VenueId = 1,
                    Rating = 4.0,
                    Comment = "Odličan izbor za toplidan večer",
                    CheckInDate = new DateTime(2024, 3, 15),
                    CreatedAt = new DateTime(2024, 3, 15, 19, 30, 0)
                },
                new CheckIn
                {
                    Id = 2,
                    UserId = 1,
                    BeerId = 2,
                    VenueId = 2,
                    Rating = 4.5,
                    Comment = "Sjajno IPA, preporučujem svima",
                    CheckInDate = new DateTime(2024, 3, 16),
                    CreatedAt = new DateTime(2024, 3, 16, 20, 15, 0)
                },
                new CheckIn
                {
                    Id = 3,
                    UserId = 2,
                    BeerId = 2,
                    VenueId = 1,
                    Rating = 5.0,
                    Comment = "Savršeno! Najbolje IPA koje sam pio",
                    CheckInDate = new DateTime(2024, 3, 17),
                    CreatedAt = new DateTime(2024, 3, 17, 21, 45, 0)
                },
                new CheckIn
                {
                    Id = 4,
                    UserId = 2,
                    BeerId = 5,
                    VenueId = 2,
                    Rating = 4.0,
                    Comment = "Jako hopno, za prave IPA ljubitelje",
                    CheckInDate = new DateTime(2024, 3, 18),
                    CreatedAt = new DateTime(2024, 3, 18, 19, 20, 0)
                },
                new CheckIn
                {
                    Id = 5,
                    UserId = 3,
                    BeerId = 3,
                    VenueId = 3,
                    Rating = 5.0,
                    Comment = "Pravi Guinness u Dublinu - nema bolega!",
                    CheckInDate = new DateTime(2024, 3, 19),
                    CreatedAt = new DateTime(2024, 3, 19, 18, 00, 0)
                },
                new CheckIn
                {
                    Id = 6,
                    UserId = 3,
                    BeerId = 1,
                    VenueId = 1,
                    Rating = 3.5,
                    Comment = "Dobro hrvatsko pivo, čvrst izbor",
                    CheckInDate = new DateTime(2024, 3, 20),
                    CreatedAt = new DateTime(2024, 3, 20, 20, 30, 0)
                },
                new CheckIn
                {
                    Id = 7,
                    UserId = 1,
                    BeerId = 3,
                    VenueId = 1,
                    Rating = 4.5,
                    Comment = "Klasičan Stout, topla preporuka",
                    CheckInDate = new DateTime(2024, 3, 21),
                    CreatedAt = new DateTime(2024, 3, 21, 19, 00, 0)
                }
            };

            // Inicijalizacija recenzija
            _reviews = new List<Review>
            {
                new Review
                {
                    Id = 1,
                    UserId = 1,
                    BeerId = 2,
                    Rating = 4.5,
                    Comment = "Odličan balanc između gorčine i arome",
                    CreatedAt = new DateTime(2024, 3, 16),
                    Likes = 12
                },
                new Review
                {
                    Id = 2,
                    UserId = 2,
                    BeerId = 2,
                    Rating = 5.0,
                    Comment = "Jedan od najboljih IPA-a koju sam ikad probao",
                    CreatedAt = new DateTime(2024, 3, 17),
                    Likes = 23
                },
                new Review
                {
                    Id = 3,
                    UserId = 3,
                    BeerId = 3,
                    Rating = 5.0,
                    Comment = "Irski stout kakav treba biti",
                    CreatedAt = new DateTime(2024, 3, 19),
                    Likes = 18
                }
            };

            // Inicijalizacija prijateljstava
            _friendships = new List<Friendship>
            {
                new Friendship
                {
                    Id = 1,
                    FromUserId = 1,
                    ToUserId = 2,
                    CreatedAt = new DateTime(2024, 1, 10)
                },
                new Friendship
                {
                    Id = 2,
                    FromUserId = 2,
                    ToUserId = 1,
                    CreatedAt = new DateTime(2024, 1, 10)
                },
                new Friendship
                {
                    Id = 3,
                    FromUserId = 1,
                    ToUserId = 3,
                    CreatedAt = new DateTime(2024, 2, 5)
                },
                new Friendship
                {
                    Id = 4,
                    FromUserId = 2,
                    ToUserId = 3,
                    CreatedAt = new DateTime(2024, 2, 15)
                }
            };
        }

        public static CuggerDataService Instance => _instance ??= new CuggerDataService();

        // Breweries
        public List<Brewery> GetAllBreweries() => _breweries;
        public Brewery? GetBreweryById(int id) => _breweries.FirstOrDefault(b => b.Id == id);

        // Beers
        public List<Beer> GetAllBeers() => _beers;
        public Beer? GetBeerById(int id) => _beers.FirstOrDefault(b => b.Id == id);
        public double GetBeerAverageRating(int beerId)
        {
            var checkIns = _checkIns.Where(ci => ci.BeerId == beerId).ToList();
            return checkIns.Count > 0 ? checkIns.Average(ci => ci.Rating) : 0.0;
        }

        // Users
        public List<User> GetAllUsers() => _users;
        public User? GetUserById(int id) => _users.FirstOrDefault(u => u.Id == id);
        public int GetUserCheckInCount(int userId) => _checkIns.Count(ci => ci.UserId == userId);
        public int GetUserFriendsCount(int userId) => _friendships.Count(f => f.FromUserId == userId);

        // CheckIns
        public List<CheckIn> GetAllCheckIns() => _checkIns.OrderByDescending(ci => ci.CreatedAt).ToList();
        public CheckIn? GetCheckInById(int id) => _checkIns.FirstOrDefault(ci => ci.Id == id);
        public List<CheckIn> GetCheckInsByUser(int userId) => _checkIns.Where(ci => ci.UserId == userId).OrderByDescending(ci => ci.CreatedAt).ToList();
        public List<CheckIn> GetCheckInsByBeer(int beerId) => _checkIns.Where(ci => ci.BeerId == beerId).OrderByDescending(ci => ci.CreatedAt).ToList();
        public List<CheckIn> GetCheckInsByVenue(int venueId) => _checkIns.Where(ci => ci.VenueId == venueId).OrderByDescending(ci => ci.CreatedAt).ToList();
        public List<CheckIn> GetRecentCheckIns(int count = 10) => _checkIns.OrderByDescending(ci => ci.CreatedAt).Take(count).ToList();

        // Reviews
        public List<Review> GetAllReviews() => _reviews;
        public Review? GetReviewById(int id) => _reviews.FirstOrDefault(r => r.Id == id);
        public List<Review> GetReviewsByBeer(int beerId) => _reviews.Where(r => r.BeerId == beerId).OrderByDescending(r => r.CreatedAt).ToList();
        public List<Review> GetReviewsByUser(int userId) => _reviews.Where(r => r.UserId == userId).OrderByDescending(r => r.CreatedAt).ToList();

        // Venues
        public List<Venue> GetAllVenues() => _venues;
        public Venue? GetVenueById(int id) => _venues.FirstOrDefault(v => v.Id == id);
        public int GetVenueCheckInCount(int venueId) => _checkIns.Count(ci => ci.VenueId == venueId);

        // Friendships
        public List<Friendship> GetAllFriendships() => _friendships;
        public Friendship? GetFriendshipById(int id) => _friendships.FirstOrDefault(f => f.Id == id);
        public List<User> GetUserFriends(int userId)
        {
            var friendIds = _friendships
                .Where(f => f.FromUserId == userId)
                .Select(f => f.ToUserId)
                .ToList();
            return _users.Where(u => friendIds.Contains(u.Id)).ToList();
        }

        // Dashboard methods
        public List<CheckIn> GetTopRecentCheckIns(int count = 5) => GetRecentCheckIns(count);
        public List<Beer> GetTopRatedBeers(int count = 5)
        {
            return _beers
                .OrderByDescending(b => GetBeerAverageRating(b.Id))
                .Take(count)
                .ToList();
        }
        public List<User> GetMostActiveUsers(int count = 5)
        {
            return _users
                .OrderByDescending(u => GetUserCheckInCount(u.Id))
                .Take(count)
                .ToList();
        }
    }
}
