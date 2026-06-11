using Cugger.Data;
using Cugger.Models;
using Cugger.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cugger.Controllers.Api
{
    /// <summary>
    /// Lab-5: REST API za entitet Beer (CRUD + pretraga, DTO).
    /// GET akcije su javne; POST/PUT/DELETE zahtijevaju rolu Admin.
    /// </summary>
    [ApiController]
    [Route("api/beers")]
    [Produces("application/json")]
    public class BeersApiController : ControllerBase
    {
        private readonly CuggerDbContext _db;

        public BeersApiController(CuggerDbContext db)
        {
            _db = db;
        }

        /// <summary>GET api/beers — svi zapisi uz pretragu (q), filtere (style, breweryId) i paging.</summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<BeerDto>>> GetAll(
            string? q, string? style, int? breweryId, string? sort,
            int page = 1, int pageSize = 50)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = _db.Beers.Include(b => b.Brewery).AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var t = q.Trim().ToLower();
                query = query.Where(b =>
                    b.Name.ToLower().Contains(t) ||
                    b.Description.ToLower().Contains(t) ||
                    (b.Brewery != null && b.Brewery.Name.ToLower().Contains(t)));
            }

            if (!string.IsNullOrEmpty(style) && Enum.TryParse<BeerStyle>(style, true, out var parsedStyle))
                query = query.Where(b => b.Style == parsedStyle);

            if (breweryId.HasValue)
                query = query.Where(b => b.BreweryId == breweryId.Value);

            query = sort?.ToLower() switch
            {
                "abv" => query.OrderByDescending(b => b.ABV),
                "ibu" => query.OrderByDescending(b => b.IBU),
                _ => query.OrderBy(b => b.Name)
            };

            var beers = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var beerIds = beers.Select(b => b.Id).ToList();
            var stats = await _db.CheckIns
                .Where(c => beerIds.Contains(c.BeerId))
                .GroupBy(c => c.BeerId)
                .Select(g => new { BeerId = g.Key, Count = g.Count(), Avg = g.Average(c => c.Rating) })
                .ToDictionaryAsync(x => x.BeerId);

            return beers
                .Select(b => stats.TryGetValue(b.Id, out var s)
                    ? b.ToDto(s.Count, s.Avg)
                    : b.ToDto())
                .ToList();
        }

        /// <summary>GET api/beers/{id} — jedan zapis po ID-u.</summary>
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<BeerDto>> GetById(int id)
        {
            var beer = await _db.Beers
                .Include(b => b.Brewery)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (beer == null)
                return NotFound(new ProblemDetails { Title = $"Pivo s ID-em {id} ne postoji.", Status = 404 });

            var ratingCount = await _db.CheckIns.CountAsync(c => c.BeerId == id);
            var avg = ratingCount > 0
                ? await _db.CheckIns.Where(c => c.BeerId == id).AverageAsync(c => c.Rating)
                : 0;

            return beer.ToDto(ratingCount, avg);
        }

        /// <summary>POST api/beers — kreiranje zapisa (Admin).</summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BeerDto>> Create(BeerInputDto input)
        {
            if (!await _db.Breweries.AnyAsync(b => b.Id == input.BreweryId))
            {
                ModelState.AddModelError(nameof(input.BreweryId), $"Pivovara s ID-em {input.BreweryId} ne postoji.");
                return ValidationProblem(ModelState);
            }

            var beer = new Beer
            {
                Name = input.Name.Trim(),
                Style = Enum.Parse<BeerStyle>(input.Style, true),
                ABV = input.Abv,
                IBU = input.Ibu,
                Description = input.Description,
                ImageUrl = input.ImageUrl,
                BreweryId = input.BreweryId
            };

            _db.Beers.Add(beer);
            await _db.SaveChangesAsync();
            await _db.Entry(beer).Reference(b => b.Brewery).LoadAsync();

            return CreatedAtAction(nameof(GetById), new { id = beer.Id }, beer.ToDto());
        }

        /// <summary>PUT api/beers/{id} — izmjena zapisa (Admin).</summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, BeerInputDto input)
        {
            var beer = await _db.Beers.FindAsync(id);
            if (beer == null)
                return NotFound(new ProblemDetails { Title = $"Pivo s ID-em {id} ne postoji.", Status = 404 });

            if (!await _db.Breweries.AnyAsync(b => b.Id == input.BreweryId))
            {
                ModelState.AddModelError(nameof(input.BreweryId), $"Pivovara s ID-em {input.BreweryId} ne postoji.");
                return ValidationProblem(ModelState);
            }

            beer.Name = input.Name.Trim();
            beer.Style = Enum.Parse<BeerStyle>(input.Style, true);
            beer.ABV = input.Abv;
            beer.IBU = input.Ibu;
            beer.Description = input.Description;
            beer.ImageUrl = input.ImageUrl;
            beer.BreweryId = input.BreweryId;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>DELETE api/beers/{id} — brisanje zapisa (Admin).</summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var beer = await _db.Beers.Include(b => b.Photos).FirstOrDefaultAsync(b => b.Id == id);
            if (beer == null)
                return NotFound(new ProblemDetails { Title = $"Pivo s ID-em {id} ne postoji.", Status = 404 });

            // Poslovno pravilo: pivo s postojećim check-inovima/recenzijama se ne briše
            var hasActivity = await _db.CheckIns.AnyAsync(c => c.BeerId == id)
                              || await _db.Reviews.AnyAsync(r => r.BeerId == id);
            if (hasActivity)
                return Conflict(new ProblemDetails
                {
                    Title = "Pivo ima postojeće check-inove ili recenzije i ne može se obrisati.",
                    Status = 409
                });

            _db.Beers.Remove(beer);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
