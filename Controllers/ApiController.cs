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

        public ApiController(CuggerDbContext db)
        {
            _db = db;
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
                query = query.Where(u => u.Username.ToLower().Contains(t) || u.FirstName.ToLower().Contains(t) || u.LastName.ToLower().Contains(t));
            }

            var results = await query
                .OrderBy(u => u.Username)
                .Take(take)
                .Select(u => new LookupResult
                {
                    Id = u.Id,
                    Label = $"{u.FirstName} {u.LastName}",
                    SubLabel = "@" + u.Username
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
                query = query.Where(u => u.Username.ToLower().Contains(t) || u.FirstName.ToLower().Contains(t) || u.LastName.ToLower().Contains(t) || u.Bio.ToLower().Contains(t));
            }
            var users = await query.OrderBy(u => u.Username).ToListAsync();
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
                    (c.User != null && (c.User.Username.ToLower().Contains(t) || c.User.FirstName.ToLower().Contains(t) || c.User.LastName.ToLower().Contains(t))) ||
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
                    (r.User != null && (r.User.Username.ToLower().Contains(t) || r.User.FirstName.ToLower().Contains(t) || r.User.LastName.ToLower().Contains(t))));
            }

            var reviews = await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
            return PartialView("_ReviewGrid", reviews);
        }
    }
}
