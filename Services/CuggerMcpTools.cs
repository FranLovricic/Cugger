using System.ComponentModel;
using System.Text.Json;
using Cugger.Data;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.Server;

namespace Cugger.Services
{
    /// <summary>
    /// MCP (Model Context Protocol) alati nad Cugger podacima.
    /// Izloženi kroz HTTP MCP endpoint (/mcp) — agentic IDE-i (Claude Code, VS Code, Cursor)
    /// mogu pretraživati piva, pivovare, lokale, check-inove i recenzije.
    /// </summary>
    [McpServerToolType]
    public class CuggerMcpTools
    {
        private readonly CuggerDbContext _db;

        public CuggerMcpTools(CuggerDbContext db) => _db = db;

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        private static string ToJson(object o) => JsonSerializer.Serialize(o, JsonOpts);

        [McpServerTool(Name = "search_beers")]
        [Description("Pretraži piva po nazivu, opisu ili pivovari. Vraća listu piva s prosječnom ocjenom. Parametri: query (tekst pretrage, opcionalno), style (stil piva: Lager, Pilsner, IPA, Stout, Porter, Ale, Wheat, Sour, Cider, Other; opcionalno), limit (broj rezultata, default 10).")]
        public async Task<string> SearchBeers(
            [Description("Tekst pretrage — naziv piva, dio opisa ili ime pivovare")] string? query = null,
            [Description("Stil piva (Lager, Pilsner, IPA, Stout, Porter, Ale, Wheat, Sour, Cider, Other)")] string? style = null,
            [Description("Maksimalan broj rezultata (1-50)")] int limit = 10)
        {
            limit = Math.Clamp(limit, 1, 50);
            var q = _db.Beers.Include(b => b.Brewery).AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                var t = query.Trim().ToLower();
                q = q.Where(b => b.Name.ToLower().Contains(t)
                              || b.Description.ToLower().Contains(t)
                              || (b.Brewery != null && b.Brewery.Name.ToLower().Contains(t)));
            }

            if (!string.IsNullOrEmpty(style) && Enum.TryParse<Models.BeerStyle>(style, true, out var parsed))
                q = q.Where(b => b.Style == parsed);

            var beers = await q.OrderBy(b => b.Name).Take(limit).ToListAsync();
            var ids = beers.Select(b => b.Id).ToList();
            var stats = await _db.CheckIns.Where(c => ids.Contains(c.BeerId))
                .GroupBy(c => c.BeerId)
                .Select(g => new { BeerId = g.Key, Count = g.Count(), Avg = g.Average(c => c.Rating) })
                .ToDictionaryAsync(x => x.BeerId);

            return ToJson(beers.Select(b => new
            {
                b.Id,
                b.Name,
                Style = b.Style.ToString(),
                b.ABV,
                b.IBU,
                Brewery = b.Brewery?.Name,
                AverageRating = stats.TryGetValue(b.Id, out var s) ? Math.Round(s.Avg, 2) : 0,
                CheckIns = stats.TryGetValue(b.Id, out var s2) ? s2.Count : 0,
                Url = $"/pivo/{b.Id}"
            }));
        }

        [McpServerTool(Name = "get_beer")]
        [Description("Dohvati detalje jednog piva po ID-u: naziv, stil, ABV, IBU, opis, pivovaru, ocjene i zadnje recenzije.")]
        public async Task<string> GetBeer(
            [Description("ID piva")] int id)
        {
            var beer = await _db.Beers.Include(b => b.Brewery).FirstOrDefaultAsync(b => b.Id == id);
            if (beer == null) return ToJson(new { error = $"Pivo s ID-em {id} ne postoji." });

            var ratingCount = await _db.CheckIns.CountAsync(c => c.BeerId == id);
            var avg = ratingCount > 0 ? await _db.CheckIns.Where(c => c.BeerId == id).AverageAsync(c => c.Rating) : 0;
            var reviews = await _db.Reviews.Include(r => r.User)
                .Where(r => r.BeerId == id)
                .OrderByDescending(r => r.CreatedAt).Take(5)
                .Select(r => new { User = r.User!.UserName, r.Rating, r.Comment, r.CreatedAt })
                .ToListAsync();

            return ToJson(new
            {
                beer.Id,
                beer.Name,
                Style = beer.Style.ToString(),
                beer.ABV,
                beer.IBU,
                beer.Description,
                Brewery = beer.Brewery == null ? null : new { beer.Brewery.Id, beer.Brewery.Name, beer.Brewery.City, beer.Brewery.Country },
                AverageRating = Math.Round(avg, 2),
                CheckInCount = ratingCount,
                RecentReviews = reviews
            });
        }

        [McpServerTool(Name = "search_breweries")]
        [Description("Pretraži pivovare po nazivu, gradu ili državi. Vraća listu pivovara s brojem piva.")]
        public async Task<string> SearchBreweries(
            [Description("Tekst pretrage")] string? query = null,
            [Description("Maksimalan broj rezultata (1-50)")] int limit = 10)
        {
            limit = Math.Clamp(limit, 1, 50);
            var q = _db.Breweries.Include(b => b.Beers).AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                var t = query.Trim().ToLower();
                q = q.Where(b => b.Name.ToLower().Contains(t) || b.City.ToLower().Contains(t) || b.Country.ToLower().Contains(t));
            }

            var breweries = await q.OrderBy(b => b.Name).Take(limit).ToListAsync();
            return ToJson(breweries.Select(b => new
            {
                b.Id, b.Name, b.City, b.Country, b.FoundedYear,
                BeerCount = b.Beers.Count,
                Url = $"/pivovara/{b.Id}"
            }));
        }

        [McpServerTool(Name = "search_venues")]
        [Description("Pretraži lokale (pubove, pivnice) po nazivu, gradu ili adresi.")]
        public async Task<string> SearchVenues(
            [Description("Tekst pretrage")] string? query = null,
            [Description("Maksimalan broj rezultata (1-50)")] int limit = 10)
        {
            limit = Math.Clamp(limit, 1, 50);
            var q = _db.Venues.AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                var t = query.Trim().ToLower();
                q = q.Where(v => v.Name.ToLower().Contains(t) || v.City.ToLower().Contains(t) || v.Address.ToLower().Contains(t));
            }

            var venues = await q.OrderBy(v => v.Name).Take(limit).ToListAsync();
            return ToJson(venues.Select(v => new { v.Id, v.Name, v.Address, v.City, v.Country }));
        }

        [McpServerTool(Name = "get_feed")]
        [Description("Dohvati najnovije check-inove (feed aktivnosti): tko je pio koje pivo, gdje i s kojom ocjenom.")]
        public async Task<string> GetFeed(
            [Description("Maksimalan broj check-inova (1-50)")] int limit = 10)
        {
            limit = Math.Clamp(limit, 1, 50);
            var checkIns = await _db.CheckIns
                .Include(c => c.User).Include(c => c.Beer).Include(c => c.Venue)
                .OrderByDescending(c => c.CreatedAt).Take(limit)
                .Select(c => new
                {
                    c.Id,
                    User = c.User!.UserName,
                    Beer = c.Beer!.Name,
                    Venue = c.Venue!.Name,
                    c.Rating,
                    c.Comment,
                    c.CheckInDate
                })
                .ToListAsync();
            return ToJson(checkIns);
        }

        [McpServerTool(Name = "get_top_beers")]
        [Description("Dohvati najbolje ocijenjena piva prema prosječnoj ocjeni check-inova.")]
        public async Task<string> GetTopBeers(
            [Description("Maksimalan broj rezultata (1-20)")] int limit = 5)
        {
            limit = Math.Clamp(limit, 1, 20);
            var top = await _db.CheckIns
                .GroupBy(c => c.BeerId)
                .Select(g => new { BeerId = g.Key, Avg = g.Average(c => c.Rating), Count = g.Count() })
                .OrderByDescending(x => x.Avg)
                .Take(limit)
                .ToListAsync();

            var ids = top.Select(t => t.BeerId).ToList();
            var beers = await _db.Beers.Include(b => b.Brewery)
                .Where(b => ids.Contains(b.Id)).ToDictionaryAsync(b => b.Id);

            return ToJson(top.Select(t => new
            {
                Id = t.BeerId,
                Name = beers.TryGetValue(t.BeerId, out var b) ? b.Name : "?",
                Brewery = beers.TryGetValue(t.BeerId, out var b2) ? b2.Brewery?.Name : null,
                AverageRating = Math.Round(t.Avg, 2),
                CheckIns = t.Count
            }));
        }

        [McpServerTool(Name = "get_stats")]
        [Description("Dohvati ukupnu statistiku Cugger aplikacije: broj piva, pivovara, lokala, korisnika, check-inova i recenzija.")]
        public async Task<string> GetStats()
        {
            return ToJson(new
            {
                Beers = await _db.Beers.CountAsync(),
                Breweries = await _db.Breweries.CountAsync(),
                Venues = await _db.Venues.CountAsync(),
                Users = await _db.Users.CountAsync(),
                CheckIns = await _db.CheckIns.CountAsync(),
                Reviews = await _db.Reviews.CountAsync()
            });
        }
    }
}
