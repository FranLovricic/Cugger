using Cugger.Data;
using Cugger.Models;
using Cugger.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cugger.Controllers.Api
{
    /// <summary>
    /// Lab-5: REST API za entitet Venue (CRUD + pretraga, DTO).
    /// GET akcije su javne; POST/PUT/DELETE zahtijevaju rolu Admin.
    /// </summary>
    [ApiController]
    [Route("api/venues")]
    [Produces("application/json")]
    public class VenuesApiController : ControllerBase
    {
        private readonly CuggerDbContext _db;

        public VenuesApiController(CuggerDbContext db)
        {
            _db = db;
        }

        /// <summary>GET api/venues — svi zapisi uz pretragu (q), filter (city) i paging.</summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<VenueDto>>> GetAll(
            string? q, string? city, int page = 1, int pageSize = 50)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = _db.Venues.AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var t = q.Trim().ToLower();
                query = query.Where(v =>
                    v.Name.ToLower().Contains(t) ||
                    v.City.ToLower().Contains(t) ||
                    v.Address.ToLower().Contains(t));
            }

            if (!string.IsNullOrWhiteSpace(city))
            {
                var c = city.Trim().ToLower();
                query = query.Where(v => v.City.ToLower() == c);
            }

            var venues = await query
                .OrderBy(v => v.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(v => new { Venue = v, CheckInCount = v.CheckIns.Count })
                .ToListAsync();

            return venues.Select(x => x.Venue.ToDto(x.CheckInCount)).ToList();
        }

        /// <summary>GET api/venues/{id} — jedan zapis po ID-u.</summary>
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<VenueDto>> GetById(int id)
        {
            var venue = await _db.Venues.FindAsync(id);
            if (venue == null)
                return NotFound(new ProblemDetails { Title = $"Lokal s ID-em {id} ne postoji.", Status = 404 });

            var checkInCount = await _db.CheckIns.CountAsync(c => c.VenueId == id);
            return venue.ToDto(checkInCount);
        }

        /// <summary>POST api/venues — kreiranje zapisa (Admin).</summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<VenueDto>> Create(VenueInputDto input)
        {
            var venue = new Venue
            {
                Name = input.Name.Trim(),
                Address = input.Address.Trim(),
                City = input.City.Trim(),
                Country = input.Country.Trim(),
                Latitude = input.Latitude,
                Longitude = input.Longitude
            };

            _db.Venues.Add(venue);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = venue.Id }, venue.ToDto());
        }

        /// <summary>PUT api/venues/{id} — izmjena zapisa (Admin).</summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, VenueInputDto input)
        {
            var venue = await _db.Venues.FindAsync(id);
            if (venue == null)
                return NotFound(new ProblemDetails { Title = $"Lokal s ID-em {id} ne postoji.", Status = 404 });

            venue.Name = input.Name.Trim();
            venue.Address = input.Address.Trim();
            venue.City = input.City.Trim();
            venue.Country = input.Country.Trim();
            venue.Latitude = input.Latitude;
            venue.Longitude = input.Longitude;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>DELETE api/venues/{id} — brisanje zapisa (Admin).</summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var venue = await _db.Venues.FindAsync(id);
            if (venue == null)
                return NotFound(new ProblemDetails { Title = $"Lokal s ID-em {id} ne postoji.", Status = 404 });

            // Poslovno pravilo: lokal s postojećim check-inovima se ne briše
            if (await _db.CheckIns.AnyAsync(c => c.VenueId == id))
                return Conflict(new ProblemDetails
                {
                    Title = "Lokal ima postojeće check-inove i ne može se obrisati.",
                    Status = 409
                });

            _db.Venues.Remove(venue);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
