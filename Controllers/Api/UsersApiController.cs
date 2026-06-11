using System.Security.Claims;
using Cugger.Data;
using Cugger.Models;
using Cugger.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cugger.Controllers.Api
{
    /// <summary>
    /// Lab-5: REST API za korisnike (CRUD + pretraga, DTO).
    /// DTO ne izlaže interna polja (email, password hash, security stampovi...).
    /// Kreiranje/brisanje korisnika je Admin operacija; profil može mijenjati
    /// vlasnik ili Admin. Kreiranje ide kroz UserManager (Identity pravila).
    /// </summary>
    [ApiController]
    [Route("api/users")]
    [Produces("application/json")]
    public class UsersApiController : ControllerBase
    {
        private readonly CuggerDbContext _db;
        private readonly UserManager<AppUser> _userManager;

        public UsersApiController(CuggerDbContext db, UserManager<AppUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        private int CurrentUserId =>
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

        /// <summary>GET api/users — svi zapisi uz pretragu (q) i paging.</summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<UserDto>>> GetAll(string? q, int page = 1, int pageSize = 50)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = _db.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var t = q.Trim().ToLower();
                query = query.Where(u =>
                    (u.UserName != null && u.UserName.ToLower().Contains(t)) ||
                    u.FirstName.ToLower().Contains(t) ||
                    u.LastName.ToLower().Contains(t) ||
                    u.Bio.ToLower().Contains(t));
            }

            var users = await query
                .OrderBy(u => u.UserName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new
                {
                    User = u,
                    CheckInCount = u.CheckIns.Count,
                    ReviewCount = u.Reviews.Count,
                    FriendCount = u.FromFriendships.Count
                })
                .ToListAsync();

            return users.Select(x => x.User.ToDto(x.CheckInCount, x.ReviewCount, x.FriendCount)).ToList();
        }

        /// <summary>GET api/users/{id} — jedan zapis po ID-u.</summary>
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<UserDto>> GetById(int id)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
                return NotFound(new ProblemDetails { Title = $"Korisnik s ID-em {id} ne postoji.", Status = 404 });

            var checkInCount = await _db.CheckIns.CountAsync(c => c.UserId == id);
            var reviewCount = await _db.Reviews.CountAsync(r => r.UserId == id);
            var friendCount = await _db.Friendships.CountAsync(f => f.FromUserId == id);

            return user.ToDto(checkInCount, reviewCount, friendCount);
        }

        /// <summary>POST api/users — kreiranje korisnika (Admin), kroz Identity UserManager.</summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserDto>> Create(UserCreateDto input)
        {
            var user = new AppUser
            {
                UserName = input.Username.Trim(),
                Email = input.Email.Trim().ToLowerInvariant(),
                EmailConfirmed = true, // admin kreira već potvrđen račun
                FirstName = input.FirstName.Trim(),
                LastName = input.LastName.Trim(),
                Bio = input.Bio,
                AvatarUrl = string.IsNullOrWhiteSpace(input.AvatarUrl)
                    ? $"https://ui-avatars.com/api/?name={Uri.EscapeDataString(input.FirstName + "+" + input.LastName)}&background=F59E0B&color=111"
                    : input.AvatarUrl,
                RegistrationDate = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, input.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return ValidationProblem(ModelState);
            }

            await _userManager.AddToRoleAsync(user, "Member");

            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user.ToDto());
        }

        /// <summary>PUT api/users/{id} — izmjena profila (vlasnik ili Admin).</summary>
        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, UserUpdateDto input)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
                return NotFound(new ProblemDetails { Title = $"Korisnik s ID-em {id} ne postoji.", Status = 404 });

            if (id != CurrentUserId && !User.IsInRole("Admin"))
                return Forbid();

            user.FirstName = input.FirstName.Trim();
            user.LastName = input.LastName.Trim();
            user.Bio = input.Bio;
            user.AvatarUrl = input.AvatarUrl;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>DELETE api/users/{id} — brisanje korisnika (Admin).</summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
                return NotFound(new ProblemDetails { Title = $"Korisnik s ID-em {id} ne postoji.", Status = 404 });

            // Poslovno pravilo: korisnik s postojećom aktivnošću se ne briše
            var hasActivity = await _db.CheckIns.AnyAsync(c => c.UserId == id)
                              || await _db.Reviews.AnyAsync(r => r.UserId == id)
                              || await _db.Friendships.AnyAsync(f => f.FromUserId == id || f.ToUserId == id);
            if (hasActivity)
                return Conflict(new ProblemDetails
                {
                    Title = "Korisnik ima postojeće check-inove, recenzije ili prijateljstva i ne može se obrisati.",
                    Status = 409
                });

            await _userManager.DeleteAsync(user);
            return NoContent();
        }
    }
}
