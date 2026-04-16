using Cugger.Models;
using Cugger.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<CuggerDataService>(CuggerDataService.Instance);

var app = builder.Build();

// ========== INICIJALIZACIJA PODATAKA ==========
Console.WriteLine("========== CUGGER - Inicijalizacija podataka ==========\n");

// Kreiramo pivaovare
var breweries = new List<Brewery>
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

// Kreiramo piva
var beers = new List<Beer>
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

// Kreiramo lokale
var venues = new List<Venue>
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

// Kreiramo korisnike
var users = new List<User>
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
        AvatarUrl = "https://example.com/avatar1.jpg"
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
        AvatarUrl = "https://example.com/avatar2.jpg"
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
        AvatarUrl = "https://example.com/avatar3.jpg"
    }
};

// Kreiramo check-inove
var checkIns = new List<CheckIn>
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

// Kreiramo recenzije
var reviews = new List<Review>
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

// Kreiramo prijateljstva (N-N relacija)
var friendships = new List<Friendship>
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

// ========== LINQ UPITI ==========
Console.WriteLine("\n========== LINQ UPITI PREKO INICIJALNIH PODATAKA ==========\n");

// UPIT 1: Dohvat svih check-inova određenog korisnika (Dragan)
Console.WriteLine("1. CHECK-INI KORISNIKA 'Dragan Marić':");
var draganCheckIns = checkIns.Where(ci => ci.UserId == 1).ToList();
Console.WriteLine($"   Dragan je imao ukupno {draganCheckIns.Count} check-ina");
foreach (var checkIn in draganCheckIns)
{
    var beerName = beers.First(b => b.Id == checkIn.BeerId).Name;
    Console.WriteLine($"   - {beerName} (Ocjena: {checkIn.Rating}/5.0)");
}

// UPIT 2: Prosječna ocjena piva (Beer)
Console.WriteLine("\n2. PROSJEČNE OCJENE PIVA:");
var beerAverages = beers.Select(b => new
{
    BeerName = b.Name,
    AverageRating = checkIns.Where(ci => ci.BeerId == b.Id).Count() > 0 
        ? checkIns.Where(ci => ci.BeerId == b.Id).Average(ci => ci.Rating)
        : 0.0,
    RatingCount = checkIns.Where(ci => ci.BeerId == b.Id).Count()
}).OrderByDescending(x => x.AverageRating).ToList();

foreach (var beer in beerAverages)
{
    Console.WriteLine($"   {beer.BeerName}: {beer.AverageRating:F1}/5.0 ({beer.RatingCount} ocjena)");
}

// UPIT 3: Najaktivniji korisnik (po broju check-inova)
Console.WriteLine("\n3. NAJAKTIVNIJI KORISNICI (po broju check-inova):");
var mostActiveUsers = users.Select(u => new
{
    UserName = $"{u.FirstName} {u.LastName}",
    CheckInCount = checkIns.Where(ci => ci.UserId == u.Id).Count(),
    ReviewCount = reviews.Where(r => r.UserId == u.Id).Count()
}).OrderByDescending(x => x.CheckInCount).ToList();

foreach (var user in mostActiveUsers)
{
    Console.WriteLine($"   {user.UserName}: {user.CheckInCount} check-ina, {user.ReviewCount} recenzija");
}

// UPIT 4: Sortiranje piva po ocjeni
Console.WriteLine("\n4. PIVA SORTIRANA PO PROSJEČNOJ OCJENI (Silazno):");
var beersSortedByRating = beers
    .Select(b => new
    {
        BeerName = b.Name,
        Style = b.Style.ToString(),
        AverageRating = checkIns.Where(ci => ci.BeerId == b.Id).Count() > 0 
            ? checkIns.Where(ci => ci.BeerId == b.Id).Average(ci => ci.Rating)
            : 0.0
    })
    .OrderByDescending(x => x.AverageRating)
    .ToList();

foreach (var beer in beersSortedByRating)
{
    Console.WriteLine($"   {beer.BeerName} ({beer.Style}): {beer.AverageRating:F1}/5.0");
}

// UPIT 5: Prijatelji određenog korisnika
Console.WriteLine("\n5. PRIJATELJE KORISNIKA 'Dragan Marić':");
var draganFriends = friendships
    .Where(f => f.FromUserId == 1)
    .Select(f => users.First(u => u.Id == f.ToUserId))
    .ToList();

foreach (var friend in draganFriends)
{
    var friendCheckInsCount = checkIns.Where(ci => ci.UserId == friend.Id).Count();
    Console.WriteLine($"   - {friend.FirstName} {friend.LastName} ({friendCheckInsCount} check-inova)");
}

// UPIT 6: Piva s najjednostavnijim stilom (samo lakeri i pilsneri)
Console.WriteLine("\n6. JEDNOSTAVNA PIVA (Lager i Pilsner):");
var simpleBeers = beers
    .Where(b => b.Style == BeerStyle.Lager || b.Style == BeerStyle.Pilsner)
    .OrderBy(b => b.ABV)
    .ToList();

foreach (var beer in simpleBeers)
{
    Console.WriteLine($"   {beer.Name}: {beer.ABV}% ABV");
}

// UPIT 7: Pregled check-inova po lokaciji
Console.WriteLine("\n7. CHECK-INI PO LOKACIJI:");
var checkInsByVenue = venues.Select(v => new
{
    VenueName = v.Name,
    City = v.City,
    CheckInCount = checkIns.Where(ci => ci.VenueId == v.Id).Count()
}).OrderByDescending(x => x.CheckInCount).ToList();

foreach (var venue in checkInsByVenue)
{
    Console.WriteLine($"   {venue.VenueName} ({venue.City}): {venue.CheckInCount} check-ina");
}

// Ispis zaključka
Console.WriteLine("\n========== ZAKLJUČAK ==========");
Console.WriteLine($"Ukupan broj korisnika: {users.Count}");
Console.WriteLine($"Ukupan broj piva: {beers.Count}");
Console.WriteLine($"Ukupan broj check-inova: {checkIns.Count}");
Console.WriteLine($"Ukupan broj prijateljstava: {friendships.Count}");

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
