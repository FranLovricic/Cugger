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
    /// Lab-5: REST API za entitet Friendship (DTO s ugniježđenim korisnicima).
    /// Poslovna pravila: prijateljstvo se kreira i briše, ali se NE uređuje
    /// (nema smislenih polja za izmjenu) — zato PUT nije implementiran.
    /// GET akcije su javne; POST/DELETE zahtijevaju prijavu (vlasnik ili Admin).
    /// </summary>
    [ApiController]
    [Route("api/friendships")]
    [Produces("application/json")]
    public class FriendshipsApiController : ControllerBase
    {
        private readonly CuggerDbContext _db;

        public FriendshipsApiController(CuggerDbContext db)
        {
            _db = db;
        }

        private int CurrentUserId =>
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

        /// <summary>GET api/friendships — svi zapisi uz filter (userId) i paging.</summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<FriendshipDto>>> GetAll(int? userId, int page = 1, int pageSize = 50)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = _db.Friendships
                .Include(f => f.FromUser)
                .Include(f => f.ToUser)
                .AsQueryable();

            if (userId.HasValue)
                query = query.Where(f => f.FromUserId == userId.Value || f.ToUserId == userId.Value);

            var friendships = await query
                .OrderByDescending(f => f.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return friendships.Select(f => f.ToDto()).ToList();
        }

        /// <summary>GET api/friendships/{id} — jedan zapis po ID-u.</summary>
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<FriendshipDto>> GetById(int id)
        {
            var friendship = await _db.Friendships
                .Include(f => f.FromUser)
                .Include(f => f.ToUser)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (friendship == null)
                return NotFound(new ProblemDetails { Title = $"Prijateljstvo s ID-em {id} ne postoji.", Status = 404 });

            return friendship.ToDto();
        }

        /// <summary>POST api/friendships — kreiranje zapisa (prijavljeni korisnik).</summary>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<FriendshipDto>> Create(FriendshipInputDto input)
        {
            var fromUserId = input.FromUserId ?? CurrentUserId;
            if (fromUserId != CurrentUserId && !User.IsInRole("Admin"))
                return Forbid();

            if (fromUserId == input.ToUserId)
            {
                ModelState.AddModelError(nameof(input.ToUserId), "Korisnik ne može biti prijatelj sam sa sobom.");
                return ValidationProblem(ModelState);
            }

            if (!await _db.Users.AnyAsync(u => u.Id == fromUserId))
            {
                ModelState.AddModelError(nameof(input.FromUserId), $"Korisnik s ID-em {fromUserId} ne postoji.");
                return ValidationProblem(ModelState);
            }
            if (!await _db.Users.AnyAsync(u => u.Id == input.ToUserId))
            {
                ModelState.AddModelError(nameof(input.ToUserId), $"Korisnik s ID-em {input.ToUserId} ne postoji.");
                return ValidationProblem(ModelState);
            }

            var exists = await _db.Friendships.AnyAsync(f =>
                f.FromUserId == fromUserId && f.ToUserId == input.ToUserId);
            if (exists)
                return Conflict(new ProblemDetails { Title = "Prijateljstvo već postoji.", Status = 409 });

            var friendship = new Friendship
            {
                FromUserId = fromUserId,
                ToUserId = input.ToUserId,
                CreatedAt = DateTime.UtcNow
            };

            _db.Friendships.Add(friendship);
            await _db.SaveChangesAsync();

            await _db.Entry(friendship).Reference(f => f.FromUser).LoadAsync();
            await _db.Entry(friendship).Reference(f => f.ToUser).LoadAsync();

            return CreatedAtAction(nameof(GetById), new { id = friendship.Id }, friendship.ToDto());
        }

        /// <summary>DELETE api/friendships/{id} — brisanje zapisa (vlasnik ili Admin).</summary>
        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var friendship = await _db.Friendships.FindAsync(id);
            if (friendship == null)
                return NotFound(new ProblemDetails { Title = $"Prijateljstvo s ID-em {id} ne postoji.", Status = 404 });

            if (friendship.FromUserId != CurrentUserId && !User.IsInRole("Admin"))
                return Forbid();

            _db.Friendships.Remove(friendship);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
