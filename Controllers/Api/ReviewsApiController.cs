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
    /// Lab-5: REST API za entitet Review (CRUD + pretraga, DTO s ugniježđenim podacima).
    /// GET akcije su javne; POST zahtijeva prijavu (Member/Admin);
    /// PUT/DELETE smije vlasnik recenzije ili Admin.
    /// </summary>
    [ApiController]
    [Route("api/reviews")]
    [Produces("application/json")]
    public class ReviewsApiController : ControllerBase
    {
        private readonly CuggerDbContext _db;

        public ReviewsApiController(CuggerDbContext db)
        {
            _db = db;
        }

        private int CurrentUserId =>
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

        /// <summary>GET api/reviews — svi zapisi uz pretragu (q), filtere (userId, beerId, minRating) i paging.</summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<ReviewDto>>> GetAll(
            string? q, int? userId, int? beerId, double? minRating,
            int page = 1, int pageSize = 50)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

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
                    (r.User != null && r.User.UserName != null && r.User.UserName.ToLower().Contains(t)));
            }

            if (userId.HasValue) query = query.Where(r => r.UserId == userId.Value);
            if (beerId.HasValue) query = query.Where(r => r.BeerId == beerId.Value);
            if (minRating.HasValue) query = query.Where(r => r.Rating >= minRating.Value);

            var reviews = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return reviews.Select(r => r.ToDto()).ToList();
        }

        /// <summary>GET api/reviews/{id} — jedan zapis po ID-u.</summary>
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<ReviewDto>> GetById(int id)
        {
            var review = await _db.Reviews
                .Include(r => r.User)
                .Include(r => r.Beer).ThenInclude(b => b!.Brewery)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (review == null)
                return NotFound(new ProblemDetails { Title = $"Recenzija s ID-em {id} ne postoji.", Status = 404 });

            return review.ToDto();
        }

        /// <summary>POST api/reviews — kreiranje zapisa (prijavljeni korisnik).</summary>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ReviewDto>> Create(ReviewInputDto input)
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

            var review = new Review
            {
                UserId = targetUserId,
                BeerId = input.BeerId,
                Rating = input.Rating,
                Comment = input.Comment,
                CreatedAt = DateTime.UtcNow,
                Likes = 0
            };

            _db.Reviews.Add(review);
            await _db.SaveChangesAsync();

            await _db.Entry(review).Reference(r => r.User).LoadAsync();
            await _db.Entry(review).Reference(r => r.Beer).LoadAsync();

            return CreatedAtAction(nameof(GetById), new { id = review.Id }, review.ToDto());
        }

        /// <summary>PUT api/reviews/{id} — izmjena zapisa (vlasnik ili Admin).</summary>
        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, ReviewInputDto input)
        {
            var review = await _db.Reviews.FindAsync(id);
            if (review == null)
                return NotFound(new ProblemDetails { Title = $"Recenzija s ID-em {id} ne postoji.", Status = 404 });

            if (review.UserId != CurrentUserId && !User.IsInRole("Admin"))
                return Forbid();

            if (!await _db.Beers.AnyAsync(b => b.Id == input.BeerId))
            {
                ModelState.AddModelError(nameof(input.BeerId), $"Pivo s ID-em {input.BeerId} ne postoji.");
                return ValidationProblem(ModelState);
            }

            review.BeerId = input.BeerId;
            review.Rating = input.Rating;
            review.Comment = input.Comment;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>DELETE api/reviews/{id} — brisanje zapisa (vlasnik ili Admin).</summary>
        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var review = await _db.Reviews.FindAsync(id);
            if (review == null)
                return NotFound(new ProblemDetails { Title = $"Recenzija s ID-em {id} ne postoji.", Status = 404 });

            if (review.UserId != CurrentUserId && !User.IsInRole("Admin"))
                return Forbid();

            _db.Reviews.Remove(review);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
