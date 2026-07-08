using Cugger.Data;
using Cugger.Models;
using Cugger.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cugger.Controllers
{
    [Route("api")]
    public class ApiController : Controller
    {
        private readonly CuggerDbContext _db;
        private readonly ILogger<ApiController> _logger;

        public ApiController(CuggerDbContext db, ILogger<ApiController> logger)
        {
            _db = db;
            _logger = logger;
        }

        // ====== Globalna pretraga — izbornici/stranice + svi entiteti ======

        /// <summary>Statični indeks stranica i izbornika aplikacije za globalnu pretragu.</summary>
        private static readonly (string Label, string Url, string Icon, string Keywords)[] PageIndex =
        {
            ("Početna", "/", "🏠", "pocetna home naslovnica dashboard"),
            ("Piva", "/Beer", "🍺", "piva beers pivo lista popis"),
            ("Pretraga piva", "/pretraga", "🔍", "pretraga search trazi trazilica"),
            ("Pivovare", "/Brewery", "🏭", "pivovare breweries pivovara"),
            ("Feed check-inova", "/feed", "📰", "feed checkin check-in aktivnost novosti"),
            ("Lokali", "/Venue", "📍", "lokali venues birtije pubovi barovi kafici"),
            ("Korisnici", "/User", "👥", "korisnici users profili ljudi"),
            ("Recenzije", "/Review", "⭐", "recenzije reviews ocjene komentari"),
            ("Top recenzije", "/Review/Top", "🏆", "top najbolje recenzije liked lajkane"),
            ("Prijateljstva", "/Friendship", "🤝", "prijateljstva friends prijatelji"),
            ("Novi check-in", "/CheckIn/Create", "➕", "novi check-in checkin dodaj kreiraj"),
            ("AI unos", "/ai", "🤖", "ai unos asistent umjetna inteligencija pametni unos"),
            ("Prijava", "/login", "🔑", "prijava login ulaz"),
            ("Registracija", "/register", "📝", "registracija register racun novi korisnik"),
        };

        /// <summary>
        /// GET api/search/global?q=... — globalna pretraga: izbornici/stranice + podaci
        /// (piva, pivovare, lokali, korisnici, check-inovi, recenzije). Vraća grupirani JSON.
        /// </summary>
        [HttpGet("search/global")]
        public async Task<IActionResult> GlobalSearch(string? q, int perGroup = 5)
        {
            var groups = new List<GlobalSearchGroup>();
            var t = (q ?? "").Trim().ToLower();

            if (t.Length == 0)
            {
                // Bez upita — vrati brze linkove (izbornik) da paleta odmah bude korisna
                groups.Add(new GlobalSearchGroup
                {
                    Name = "Stranice",
                    Items = PageIndex.Take(8).Select(p => new GlobalSearchItem
                    {
                        Label = p.Label, Url = p.Url, Icon = p.Icon
                    }).ToList()
                });
                return Json(new { query = q, groups });
            }

            _logger.LogInformation("Globalna pretraga: {Query}", q);

            // 1) Stranice i izbornici
            var pages = PageIndex
                .Where(p => p.Label.ToLower().Contains(t) || p.Keywords.Contains(t))
                .Take(perGroup)
                .Select(p => new GlobalSearchItem { Label = p.Label, Url = p.Url, Icon = p.Icon })
                .ToList();
            if (pages.Count > 0)
                groups.Add(new GlobalSearchGroup { Name = "Stranice", Items = pages });

            // 2) Piva
            var beers = await _db.Beers.Include(b => b.Brewery)
                .Where(b => b.Name.ToLower().Contains(t) || b.Description.ToLower().Contains(t) ||
                            (b.Brewery != null && b.Brewery.Name.ToLower().Contains(t)))
                .OrderBy(b => b.Name).Take(perGroup)
                .Select(b => new GlobalSearchItem
                {
                    Label = b.Name,
                    SubLabel = b.Brewery != null ? $"{b.Brewery.Name} · {b.Style}" : b.Style.ToString(),
                    Url = $"/pivo/{b.Id}",
                    Icon = "🍺"
                }).ToListAsync();
            if (beers.Count > 0)
                groups.Add(new GlobalSearchGroup { Name = "Piva", Items = beers });

            // 3) Pivovare
            var breweries = await _db.Breweries
                .Where(b => b.Name.ToLower().Contains(t) || b.City.ToLower().Contains(t) || b.Country.ToLower().Contains(t))
                .OrderBy(b => b.Name).Take(perGroup)
                .Select(b => new GlobalSearchItem
                {
                    Label = b.Name,
                    SubLabel = $"{b.City}, {b.Country}",
                    Url = $"/pivovara/{b.Id}",
                    Icon = "🏭"
                }).ToListAsync();
            if (breweries.Count > 0)
                groups.Add(new GlobalSearchGroup { Name = "Pivovare", Items = breweries });

            // 4) Lokali
            var venues = await _db.Venues
                .Where(v => v.Name.ToLower().Contains(t) || v.City.ToLower().Contains(t) || v.Address.ToLower().Contains(t))
                .OrderBy(v => v.Name).Take(perGroup)
                .Select(v => new GlobalSearchItem
                {
                    Label = v.Name,
                    SubLabel = $"{v.Address}, {v.City}",
                    Url = $"/Venue/Details/{v.Id}",
                    Icon = "📍"
                }).ToListAsync();
            if (venues.Count > 0)
                groups.Add(new GlobalSearchGroup { Name = "Lokali", Items = venues });

            // 5) Korisnici
            var users = await _db.Users
                .Where(u => u.UserName!.ToLower().Contains(t) || u.FirstName.ToLower().Contains(t) || u.LastName.ToLower().Contains(t))
                .OrderBy(u => u.UserName).Take(perGroup)
                .Select(u => new GlobalSearchItem
                {
                    Label = $"{u.FirstName} {u.LastName}",
                    SubLabel = "@" + u.UserName,
                    Url = $"/korisnik/{u.UserName}",
                    Icon = "👤"
                }).ToListAsync();
            if (users.Count > 0)
                groups.Add(new GlobalSearchGroup { Name = "Korisnici", Items = users });

            // 6) Check-inovi
            var checkIns = await _db.CheckIns.Include(c => c.User).Include(c => c.Beer)
                .Where(c => c.Comment.ToLower().Contains(t) ||
                            (c.Beer != null && c.Beer.Name.ToLower().Contains(t)))
                .OrderByDescending(c => c.CreatedAt).Take(perGroup)
                .Select(c => new GlobalSearchItem
                {
                    Label = c.Beer != null ? $"Check-in: {c.Beer.Name}" : "Check-in",
                    SubLabel = c.User != null ? $"@{c.User.UserName} · {c.Comment}" : c.Comment,
                    Url = $"/CheckIn/Details/{c.Id}",
                    Icon = "✅"
                }).ToListAsync();
            if (checkIns.Count > 0)
                groups.Add(new GlobalSearchGroup { Name = "Check-inovi", Items = checkIns });

            // 7) Recenzije
            var reviews = await _db.Reviews.Include(r => r.User).Include(r => r.Beer)
                .Where(r => r.Comment.ToLower().Contains(t) ||
                            (r.Beer != null && r.Beer.Name.ToLower().Contains(t)))
                .OrderByDescending(r => r.CreatedAt).Take(perGroup)
                .Select(r => new GlobalSearchItem
                {
                    Label = r.Beer != null ? $"Recenzija: {r.Beer.Name}" : "Recenzija",
                    SubLabel = r.User != null ? $"@{r.User.UserName} · {r.Rating:0.#}★ · {r.Comment}" : r.Comment,
                    Url = $"/Review/Details/{r.Id}",
                    Icon = "⭐"
                }).ToListAsync();
            if (reviews.Count > 0)
                groups.Add(new GlobalSearchGroup { Name = "Recenzije", Items = reviews });

            return Json(new { query = q, groups });
        }

        // ====== Autocomplete lookups (vraćaju [{ id, label, subLabel }]) ======

        [HttpGet("lookup/beers")]
        public async Task<IActionResult> LookupBeers(string? q, int take = 20)
        {
            var query = _db.Beers
                .Include(b => b.Brewery)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var t = q.Trim().ToLower();
                query = query.Where(b => b.Name.ToLower().Contains(t) || (b.Brewery != null && b.Brewery.Name.ToLower().Contains(t)));
            }

            var results = await query
                .OrderBy(b => b.Name)
                .Take(take)
                .Select(b => new LookupResult
                {
                    Id = b.Id,
                    Label = b.Name,
                    SubLabel = b.Brewery != null ? $"{b.Brewery.Name} · {b.Style}" : b.Style.ToString()
                })
                .ToListAsync();

            return Json(results);
        }

        [HttpGet("lookup/breweries")]
        public async Task<IActionResult> LookupBreweries(string? q, int take = 20)
        {
            var query = _db.Breweries.AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
            {
                var t = q.Trim().ToLower();
                query = query.Where(b => b.Name.ToLower().Contains(t) || b.City.ToLower().Contains(t) || b.Country.ToLower().Contains(t));
            }

            var results = await query
                .OrderBy(b => b.Name)
                .Take(take)
                .Select(b => new LookupResult
                {
                    Id = b.Id,
                    Label = b.Name,
                    SubLabel = $"{b.City}, {b.Country}"
                })
                .ToListAsync();

            return Json(results);
        }

        [HttpGet("lookup/venues")]
        public async Task<IActionResult> LookupVenues(string? q, int take = 20)
        {
            var query = _db.Venues.AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
            {
                var t = q.Trim().ToLower();
                query = query.Where(v => v.Name.ToLower().Contains(t) || v.City.ToLower().Contains(t) || v.Address.ToLower().Contains(t));
            }

            var results = await query
                .OrderBy(v => v.Name)
                .Take(take)
                .Select(v => new LookupResult
                {
                    Id = v.Id,
                    Label = v.Name,
                    SubLabel = $"{v.Address}, {v.City}"
                })
                .ToListAsync();

            return Json(results);
        }

        [HttpGet("lookup/users")]
        public async Task<IActionResult> LookupUsers(string? q, int take = 20)
        {
            var query = _db.Users.AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
            {
                var t = q.Trim().ToLower();
                query = query.Where(u => u.UserName!.ToLower().Contains(t) || u.FirstName.ToLower().Contains(t) || u.LastName.ToLower().Contains(t));
            }

            var results = await query
                .OrderBy(u => u.UserName)
                .Take(take)
                .Select(u => new LookupResult
                {
                    Id = u.Id,
                    Label = $"{u.FirstName} {u.LastName}",
                    SubLabel = "@" + u.UserName
                })
                .ToListAsync();

            return Json(results);
        }

        // ====== AJAX search — vraća partial HTML za list grid ======

        [HttpGet("search/beers")]
        public async Task<IActionResult> SearchBeers(string? q, string? style)
        {
            var query = _db.Beers
                .Include(b => b.Brewery)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var t = q.Trim().ToLower();
                query = query.Where(b => b.Name.ToLower().Contains(t) || b.Description.ToLower().Contains(t));
            }

            if (!string.IsNullOrEmpty(style) && Enum.TryParse<BeerStyle>(style, true, out var parsed))
            {
                query = query.Where(b => b.Style == parsed);
            }

            var beers = await query.OrderBy(b => b.Name).ToListAsync();

            foreach (var b in beers)
            {
                b.RatingCount = _db.CheckIns.Count(c => c.BeerId == b.Id);
                b.AverageRating = b.RatingCount > 0
                    ? _db.CheckIns.Where(c => c.BeerId == b.Id).Average(c => c.Rating)
                    : 0;
            }

            return PartialView("_BeerGrid", beers);
        }

        [HttpGet("search/breweries")]
        public async Task<IActionResult> SearchBreweries(string? q)
        {
            var query = _db.Breweries.AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
            {
                var t = q.Trim().ToLower();
                query = query.Where(b => b.Name.ToLower().Contains(t) || b.City.ToLower().Contains(t) || b.Country.ToLower().Contains(t) || b.Description.ToLower().Contains(t));
            }
            var breweries = await query.OrderBy(b => b.Name).ToListAsync();
            return PartialView("_BreweryGrid", breweries);
        }

        [HttpGet("search/venues")]
        public async Task<IActionResult> SearchVenues(string? q)
        {
            var query = _db.Venues.AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
            {
                var t = q.Trim().ToLower();
                query = query.Where(v => v.Name.ToLower().Contains(t) || v.City.ToLower().Contains(t) || v.Address.ToLower().Contains(t));
            }
            var venues = await query.OrderBy(v => v.Name).ToListAsync();
            return PartialView("_VenueGrid", venues);
        }

        [HttpGet("search/users")]
        public async Task<IActionResult> SearchUsers(string? q)
        {
            var query = _db.Users.AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
            {
                var t = q.Trim().ToLower();
                query = query.Where(u => u.UserName!.ToLower().Contains(t) || u.FirstName.ToLower().Contains(t) || u.LastName.ToLower().Contains(t) || u.Bio.ToLower().Contains(t));
            }
            var users = await query.OrderBy(u => u.UserName).ToListAsync();
            return PartialView("_UserList", users);
        }

        [HttpGet("search/checkins")]
        public async Task<IActionResult> SearchCheckIns(string? q)
        {
            var query = _db.CheckIns
                .Include(c => c.User)
                .Include(c => c.Beer).ThenInclude(b => b!.Brewery)
                .Include(c => c.Venue)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var t = q.Trim().ToLower();
                query = query.Where(c =>
                    c.Comment.ToLower().Contains(t) ||
                    (c.Beer != null && c.Beer.Name.ToLower().Contains(t)) ||
                    (c.User != null && (c.User.UserName!.ToLower().Contains(t) || c.User.FirstName.ToLower().Contains(t) || c.User.LastName.ToLower().Contains(t))) ||
                    (c.Venue != null && c.Venue.Name.ToLower().Contains(t)));
            }

            var checkIns = await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
            return PartialView("_CheckInGrid", checkIns);
        }

        [HttpGet("search/reviews")]
        public async Task<IActionResult> SearchReviews(string? q)
        {
            var query = _db.Reviews
                .Include(r => r.User)
                .Include(r => r.Beer).ThenInclude(b => b!.Brewery)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var t = q.Trim().ToLower();
                query = query.Where(r =>
                    r.Comment.ToLower().Contains(t) ||
                    (r.Beer != null && r.Beer.Name.ToLower().Contains(t)) ||
                    (r.User != null && (r.User.UserName!.ToLower().Contains(t) || r.User.FirstName.ToLower().Contains(t) || r.User.LastName.ToLower().Contains(t))));
            }

            var reviews = await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
            return PartialView("_ReviewGrid", reviews);
        }
    }
}
