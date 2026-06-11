using Cugger.Data;
using Cugger.Models;
using Cugger.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cugger.Controllers.Api
{
    /// <summary>
    /// Lab-5: REST API za entitet Brewery (CRUD + pretraga, DTO).
    /// GET akcije su javne; POST/PUT/DELETE zahtijevaju rolu Admin.
    /// </summary>
    [ApiController]
    [Route("api/breweries")]
    [Produces("application/json")]
    public class BreweriesApiController : ControllerBase
    {
        private readonly CuggerDbContext _db;

        public BreweriesApiController(CuggerDbContext db)
        {
            _db = db;
        }

        /// <summary>GET api/breweries — svi zapisi uz pretragu (q), filter (country) i paging.</summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<BreweryDto>>> GetAll(
            string? q, string? country, int page = 1, int pageSize = 50)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = _db.Breweries.AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var t = q.Trim().ToLower();
                query = query.Where(b =>
                    b.Name.ToLower().Contains(t) ||
                    b.City.ToLower().Contains(t) ||
                    b.Country.ToLower().Contains(t) ||
                    b.Description.ToLower().Contains(t));
            }

            if (!string.IsNullOrWhiteSpace(country))
            {
                var c = country.Trim().ToLower();
                query = query.Where(b => b.Country.ToLower() == c);
            }

            var breweries = await query
                .OrderBy(b => b.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new { Brewery = b, BeerCount = b.Beers.Count })
                .ToListAsync();

            return breweries.Select(x => x.Brewery.ToDto(x.BeerCount)).ToList();
        }

        /// <summary>GET api/breweries/{id} — jedan zapis po ID-u, s ugniježđenim pivima.</summary>
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<BreweryDto>> GetById(int id)
        {
            var brewery = await _db.Breweries
                .Include(b => b.Beers)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (brewery == null)
                return NotFound(new ProblemDetails { Title = $"Pivovara s ID-em {id} ne postoji.", Status = 404 });

            return brewery.ToDto(brewery.Beers.Count, includeBeers: true);
        }

        /// <summary>POST api/breweries — kreiranje zapisa (Admin).</summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BreweryDto>> Create(BreweryInputDto input)
        {
            var brewery = new Brewery
            {
                Name = input.Name.Trim(),
                Country = input.Country.Trim(),
                City = input.City.Trim(),
                FoundedYear = input.FoundedYear,
                Description = input.Description,
                WebsiteUrl = input.WebsiteUrl,
                LogoUrl = input.LogoUrl
            };

            _db.Breweries.Add(brewery);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = brewery.Id }, brewery.ToDto());
        }

        /// <summary>PUT api/breweries/{id} — izmjena zapisa (Admin).</summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, BreweryInputDto input)
        {
            var brewery = await _db.Breweries.FindAsync(id);
            if (brewery == null)
                return NotFound(new ProblemDetails { Title = $"Pivovara s ID-em {id} ne postoji.", Status = 404 });

            brewery.Name = input.Name.Trim();
            brewery.Country = input.Country.Trim();
            brewery.City = input.City.Trim();
            brewery.FoundedYear = input.FoundedYear;
            brewery.Description = input.Description;
            brewery.WebsiteUrl = input.WebsiteUrl;
            brewery.LogoUrl = input.LogoUrl;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>DELETE api/breweries/{id} — brisanje zapisa (Admin).</summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var brewery = await _db.Breweries.Include(b => b.Beers).FirstOrDefaultAsync(b => b.Id == id);
            if (brewery == null)
                return NotFound(new ProblemDetails { Title = $"Pivovara s ID-em {id} ne postoji.", Status = 404 });

            // Poslovno pravilo: pivovara čija piva imaju check-inove/recenzije se ne briše
            var beerIds = brewery.Beers.Select(b => b.Id).ToList();
            var hasActivity = await _db.CheckIns.AnyAsync(c => beerIds.Contains(c.BeerId))
                              || await _db.Reviews.AnyAsync(r => beerIds.Contains(r.BeerId));
            if (hasActivity)
                return Conflict(new ProblemDetails
                {
                    Title = "Pivovara ima piva s postojećim check-inovima ili recenzijama i ne može se obrisati.",
                    Status = 409
                });

            _db.Breweries.Remove(brewery); // cascade briše i piva
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
