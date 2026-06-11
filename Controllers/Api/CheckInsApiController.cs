using System.Security.Claims;
using Cugger.Data;
using Cugger.Models;
using Cugger.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cugger.Controllers.Api
{
    /// <summary>
    /// Lab-5: REST API za entitet CheckIn (CRUD + pretraga, DTO s ugniježđenim podacima).
    /// GET akcije su javne; POST zahtijeva prijavu (Member/Admin);
    /// PUT/DELETE smije vlasnik check-ina ili Admin.
    /// </summary>
    [ApiController]
    [Route("api/checkins")]
    [Produces("application/json")]
    public class CheckInsApiController : ControllerBase
    {
        private readonly CuggerDbContext _db;

        public CheckInsApiController(CuggerDbContext db)
        {
            _db = db;
        }

        private int CurrentUserId =>
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

        /// <summary>GET api/checkins — svi zapisi uz pretragu (q), filtere (userId, beerId, venueId) i paging.</summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<CheckInDto>>> GetAll(
            string? q, int? userId, int? beerId, int? venueId,
            int page = 1, int pageSize = 50)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

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
                    (c.Venue != null && c.Venue.Name.ToLower().Contains(t)) ||
                    (c.User != null && c.User.UserName != null && c.User.UserName.ToLower().Contains(t)));
            }

            if (userId.HasValue) query = query.Where(c => c.UserId == userId.Value);
            if (beerId.HasValue) query = query.Where(c => c.BeerId == beerId.Value);
            if (venueId.HasValue) query = query.Where(c => c.VenueId == venueId.Value);

            var checkIns = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return checkIns.Select(c => c.ToDto()).ToList();
        }

        /// <summary>GET api/checkins/{id} — jedan zapis po ID-u.</summary>
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<CheckInDto>> GetById(int id)
        {
            var checkIn = await _db.CheckIns
                .Include(c => c.User)
                .Include(c => c.Beer).ThenInclude(b => b!.Brewery)
                .Include(c => c.Venue)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (checkIn == null)
                return NotFound(new ProblemDetails { Title = $"Check-in s ID-em {id} ne postoji.", Status = 404 });

            return checkIn.ToDto();
        }

        /// <summary>POST api/checkins — kreiranje zapisa (prijavljeni korisnik).</summary>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<CheckInDto>> Create(CheckInInputDto input)
        {
            var targetUserId = input.UserId ?? CurrentUserId;
            if (targetUserId != CurrentUserId && !User.IsInRole("Admin"))
                return Forbid();

            if (!await _db.Users.AnyAsync(u => u.Id == targetUserId))
            {
                ModelState.AddModelError(nameof(input.UserId), $"Korisnik s ID-em {targetUserId} ne postoji.");
                return ValidationProblem(ModelState);
            }
            if (!await _db.Beers.AnyAsync(b => b.Id == input.BeerId))
            {
                ModelState.AddModelError(nameof(input.BeerId), $"Pivo s ID-em {input.BeerId} ne postoji.");
                return ValidationProblem(ModelState);
            }
            if (!await _db.Venues.AnyAsync(v => v.Id == input.VenueId))
            {
                ModelState.AddModelError(nameof(input.VenueId), $"Lokal s ID-em {input.VenueId} ne postoji.");
                return ValidationProblem(ModelState);
            }

            var checkIn = new CheckIn
            {
                UserId = targetUserId,
                BeerId = input.BeerId,
                VenueId = input.VenueId,
                Rating = input.Rating,
                Comment = input.Comment,
                CheckInDate = input.CheckInDate ?? DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _db.CheckIns.Add(checkIn);
            await _db.SaveChangesAsync();

            await _db.Entry(checkIn).Reference(c => c.User).LoadAsync();
            await _db.Entry(checkIn).Reference(c => c.Beer).LoadAsync();
            await _db.Entry(checkIn).Reference(c => c.Venue).LoadAsync();

            return CreatedAtAction(nameof(GetById), new { id = checkIn.Id }, checkIn.ToDto());
        }

        /// <summary>PUT api/checkins/{id} — izmjena zapisa (vlasnik ili Admin).</summary>
        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, CheckInInputDto input)
        {
            var checkIn = await _db.CheckIns.FindAsync(id);
            if (checkIn == null)
                return NotFound(new ProblemDetails { Title = $"Check-in s ID-em {id} ne postoji.", Status = 404 });

            if (checkIn.UserId != CurrentUserId && !User.IsInRole("Admin"))
                return Forbid();

            if (!await _db.Beers.AnyAsync(b => b.Id == input.BeerId))
            {
                ModelState.AddModelError(nameof(input.BeerId), $"Pivo s ID-em {input.BeerId} ne postoji.");
                return ValidationProblem(ModelState);
            }
            if (!await _db.Venues.AnyAsync(v => v.Id == input.VenueId))
            {
                ModelState.AddModelError(nameof(input.VenueId), $"Lokal s ID-em {input.VenueId} ne postoji.");
                return ValidationProblem(ModelState);
            }

            checkIn.BeerId = input.BeerId;
            checkIn.VenueId = input.VenueId;
            checkIn.Rating = input.Rating;
            checkIn.Comment = input.Comment;
            if (input.CheckInDate.HasValue)
                checkIn.CheckInDate = input.CheckInDate.Value;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>DELETE api/checkins/{id} — brisanje zapisa (vlasnik ili Admin).</summary>
        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var checkIn = await _db.CheckIns.FindAsync(id);
            if (checkIn == null)
                return NotFound(new ProblemDetails { Title = $"Check-in s ID-em {id} ne postoji.", Status = 404 });

            if (checkIn.UserId != CurrentUserId && !User.IsInRole("Admin"))
                return Forbid();

            _db.CheckIns.Remove(checkIn);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
