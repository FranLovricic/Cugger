using System.Security.Claims;
using Cugger.Data;
using Cugger.Models;
using Cugger.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cugger.Controllers
{
    /// <summary>
    /// AI integracija: unos podataka putem upita na prirodnom jeziku.
    /// Tok: korisnik upiše upit → /ai/parse vrati strukturirani prijedlog →
    /// korisnik potvrdi → /ai/create spremi entitet u bazu.
    /// </summary>
    [Authorize]
    [Route("ai")]
    public class AiController : Controller
    {
        private readonly CuggerDbContext _db;
        private readonly AiEntryService _ai;
        private readonly ILogger<AiController> _logger;

        public AiController(CuggerDbContext db, AiEntryService ai, ILogger<AiController> logger)
        {
            _db = db;
            _ai = ai;
            _logger = logger;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            ViewBag.IsConfigured = _ai.IsConfigured;
            ViewBag.IsAdmin = User.IsInRole("Admin");
            ViewBag.Breadcrumbs = new[]
            {
                new BreadcrumbItem("Početna", "/", false),
                new BreadcrumbItem("AI unos", "/ai", true)
            };
            return View();
        }

        public class ParseRequest
        {
            public string Prompt { get; set; } = "";
        }

        /// <summary>POST /ai/parse — parsira upit u prijedlog unosa (bez spremanja).</summary>
        [HttpPost("parse")]
        public async Task<IActionResult> Parse([FromBody] ParseRequest req)
        {
            if (string.IsNullOrWhiteSpace(req?.Prompt))
                return BadRequest(new { error = "Upit je prazan." });

            if (!_ai.IsConfigured)
                return StatusCode(503, new
                {
                    error = "AI nije konfiguriran. Postavi 'Anthropic:ApiKey' u appsettings.json ili ANTHROPIC_API_KEY varijablu okoline pa ponovno pokreni aplikaciju."
                });

            try
            {
                var result = await _ai.ParseAsync(req.Prompt.Trim());
                var (canCreate, problems) = await ValidateAsync(result);
                return Json(new { parsed = result, canCreate, problems });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI unos: greška pri parsiranju upita");
                return StatusCode(502, new { error = $"Greška pri pozivu AI servisa: {ex.Message}" });
            }
        }

        /// <summary>POST /ai/create — sprema potvrđeni prijedlog u bazu.</summary>
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] AiParseResult parsed)
        {
            if (parsed == null || string.IsNullOrWhiteSpace(parsed.EntityType))
                return BadRequest(new { error = "Nedostaje prijedlog unosa." });

            var (canCreate, problems) = await ValidateAsync(parsed);
            if (!canCreate)
                return BadRequest(new { error = string.Join(" ", problems) });

            var userId = GetCurrentUserId();
            if (userId == null) return Forbid();

            switch (parsed.EntityType)
            {
                case "beer":
                {
                    var brewery = await FindBreweryAsync(parsed.BreweryName!);
                    var beer = new Beer
                    {
                        Name = parsed.Name!.Trim(),
                        Style = Enum.Parse<BeerStyle>(parsed.Style ?? "Other", true),
                        ABV = parsed.Abv ?? 0,
                        IBU = parsed.Ibu ?? 0,
                        Description = parsed.Description ?? "",
                        ImageUrl = "",
                        BreweryId = brewery!.Id
                    };
                    _db.Beers.Add(beer);
                    await _db.SaveChangesAsync();
                    _logger.LogInformation("AI unos: kreirano pivo {Name} (ID {Id})", beer.Name, beer.Id);
                    return Json(new { url = $"/pivo/{beer.Id}", label = $"Pivo \"{beer.Name}\" je kreirano." });
                }
                case "brewery":
                {
                    var brewery = new Brewery
                    {
                        Name = parsed.Name!.Trim(),
                        Country = parsed.Country ?? "Hrvatska",
                        City = parsed.City ?? "",
                        FoundedYear = parsed.FoundedYear ?? 0,
                        Description = parsed.Description ?? "",
                        WebsiteUrl = parsed.WebsiteUrl ?? "",
                        LogoUrl = ""
                    };
                    _db.Breweries.Add(brewery);
                    await _db.SaveChangesAsync();
                    _logger.LogInformation("AI unos: kreirana pivovara {Name} (ID {Id})", brewery.Name, brewery.Id);
                    return Json(new { url = $"/pivovara/{brewery.Id}", label = $"Pivovara \"{brewery.Name}\" je kreirana." });
                }
                case "venue":
                {
                    var venue = new Venue
                    {
                        Name = parsed.Name!.Trim(),
                        Address = parsed.Address ?? "",
                        City = parsed.City ?? "",
                        Country = parsed.Country ?? "Hrvatska",
                        Latitude = parsed.Latitude ?? 0,
                        Longitude = parsed.Longitude ?? 0
                    };
                    _db.Venues.Add(venue);
                    await _db.SaveChangesAsync();
                    _logger.LogInformation("AI unos: kreiran lokal {Name} (ID {Id})", venue.Name, venue.Id);
                    return Json(new { url = $"/Venue/Details/{venue.Id}", label = $"Lokal \"{venue.Name}\" je kreiran." });
                }
                case "checkin":
                {
                    var beer = await FindBeerAsync(parsed.BeerName!);
                    var venue = string.IsNullOrWhiteSpace(parsed.VenueName) ? null : await FindVenueAsync(parsed.VenueName);
                    var fallbackVenue = venue ?? await _db.Venues.OrderBy(v => v.Id).FirstOrDefaultAsync();
                    if (fallbackVenue == null)
                        return BadRequest(new { error = "U bazi nema nijednog lokala za check-in." });

                    var checkIn = new CheckIn
                    {
                        UserId = userId.Value,
                        BeerId = beer!.Id,
                        VenueId = fallbackVenue.Id,
                        Rating = Math.Clamp(parsed.Rating ?? 0, 0, 5),
                        Comment = parsed.Comment ?? "",
                        CheckInDate = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow
                    };
                    _db.CheckIns.Add(checkIn);
                    await _db.SaveChangesAsync();
                    _logger.LogInformation("AI unos: kreiran check-in za pivo {Beer} (ID {Id})", beer.Name, checkIn.Id);
                    return Json(new { url = $"/CheckIn/Details/{checkIn.Id}", label = $"Check-in za \"{beer.Name}\" je kreiran." });
                }
                case "review":
                {
                    var beer = await FindBeerAsync(parsed.BeerName!);
                    var review = new Review
                    {
                        UserId = userId.Value,
                        BeerId = beer!.Id,
                        Rating = Math.Clamp(parsed.Rating ?? 0, 0, 5),
                        Comment = parsed.Comment ?? "",
                        CreatedAt = DateTime.UtcNow,
                        Likes = 0
                    };
                    _db.Reviews.Add(review);
                    await _db.SaveChangesAsync();
                    _logger.LogInformation("AI unos: kreirana recenzija za pivo {Beer} (ID {Id})", beer.Name, review.Id);
                    return Json(new { url = $"/Review/Details/{review.Id}", label = $"Recenzija za \"{beer.Name}\" je kreirana." });
                }
                default:
                    return BadRequest(new { error = "Nepoznata vrsta unosa." });
            }
        }

        /// <summary>Validira prijedlog: obavezna polja, postojanje referenci i ovlasti korisnika.</summary>
        private async Task<(bool canCreate, List<string> problems)> ValidateAsync(AiParseResult parsed)
        {
            var problems = new List<string>();
            var isAdmin = User.IsInRole("Admin");

            switch (parsed.EntityType)
            {
                case "beer":
                    if (!isAdmin) problems.Add("Samo admin može dodavati piva.");
                    if (string.IsNullOrWhiteSpace(parsed.Name)) problems.Add("Nedostaje naziv piva.");
                    if (string.IsNullOrWhiteSpace(parsed.BreweryName))
                        problems.Add("Nedostaje pivovara.");
                    else if (await FindBreweryAsync(parsed.BreweryName) == null)
                        problems.Add($"Pivovara \"{parsed.BreweryName}\" ne postoji u bazi — prvo je dodaj.");
                    break;

                case "brewery":
                    if (!isAdmin) problems.Add("Samo admin može dodavati pivovare.");
                    if (string.IsNullOrWhiteSpace(parsed.Name)) problems.Add("Nedostaje naziv pivovare.");
                    break;

                case "venue":
                    if (!isAdmin) problems.Add("Samo admin može dodavati lokale.");
                    if (string.IsNullOrWhiteSpace(parsed.Name)) problems.Add("Nedostaje naziv lokala.");
                    break;

                case "checkin":
                case "review":
                    if (string.IsNullOrWhiteSpace(parsed.BeerName))
                        problems.Add("Nedostaje naziv piva.");
                    else if (await FindBeerAsync(parsed.BeerName) == null)
                        problems.Add($"Pivo \"{parsed.BeerName}\" ne postoji u bazi.");
                    break;

                default:
                    problems.Add("Upit nije prepoznat kao unos podataka.");
                    break;
            }

            return (problems.Count == 0, problems);
        }

        private Task<Brewery?> FindBreweryAsync(string name)
        {
            var t = name.Trim().ToLower();
            return _db.Breweries.FirstOrDefaultAsync(b => b.Name.ToLower() == t || b.Name.ToLower().Contains(t));
        }

        private Task<Beer?> FindBeerAsync(string name)
        {
            var t = name.Trim().ToLower();
            return _db.Beers.FirstOrDefaultAsync(b => b.Name.ToLower() == t || b.Name.ToLower().Contains(t));
        }

        private Task<Venue?> FindVenueAsync(string name)
        {
            var t = name.Trim().ToLower();
            return _db.Venues.FirstOrDefaultAsync(v => v.Name.ToLower() == t || v.Name.ToLower().Contains(t));
        }

        private int? GetCurrentUserId()
        {
            var v = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(v, out var id) ? id : null;
        }
    }
}
