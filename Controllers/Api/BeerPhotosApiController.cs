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
    /// Lab-5: upload datoteka vezanih uz konkretno pivo (Dropzone na Beer/Details).
    /// Datoteke se spremaju na disk (wwwroot/uploads/beers/{beerId}/),
    /// a u bazu idu metapodaci + relativna putanja (BeerPhoto).
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    public class BeerPhotosApiController : ControllerBase
    {
        private static readonly string[] AllowedExtensions =
            { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".pdf" };

        private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

        private readonly CuggerDbContext _db;
        private readonly IWebHostEnvironment _env;

        public BeerPhotosApiController(CuggerDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        private int CurrentUserId =>
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

        /// <summary>GET api/beers/{beerId}/photos — AJAX popis datoteka uz pivo.</summary>
        [HttpGet("api/beers/{beerId:int}/photos")]
        [AllowAnonymous]
        public async Task<ActionResult<List<BeerPhotoDto>>> GetForBeer(int beerId)
        {
            if (!await _db.Beers.AnyAsync(b => b.Id == beerId))
                return NotFound(new ProblemDetails { Title = $"Pivo s ID-em {beerId} ne postoji.", Status = 404 });

            var photos = await _db.BeerPhotos
                .Include(p => p.UploadedBy)
                .Where(p => p.BeerId == beerId)
                .OrderByDescending(p => p.UploadedAt)
                .ToListAsync();

            return photos.Select(p => p.ToDto()).ToList();
        }

        /// <summary>POST api/beers/{beerId}/photos — async upload datoteke (Dropzone šalje polje "file").</summary>
        [HttpPost("api/beers/{beerId:int}/photos")]
        [Authorize]
        [RequestSizeLimit(MaxFileSizeBytes * 2)]
        public async Task<ActionResult<BeerPhotoDto>> Upload(int beerId, IFormFile? file)
        {
            if (!await _db.Beers.AnyAsync(b => b.Id == beerId))
                return NotFound(new ProblemDetails { Title = $"Pivo s ID-em {beerId} ne postoji.", Status = 404 });

            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("file", "Datoteka nije poslana ili je prazna.");
                return ValidationProblem(ModelState);
            }

            if (file.Length > MaxFileSizeBytes)
            {
                ModelState.AddModelError("file", "Datoteka je veća od dopuštenih 10 MB.");
                return ValidationProblem(ModelState);
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                ModelState.AddModelError("file",
                    $"Tip datoteke '{extension}' nije dopušten. Dopušteno: {string.Join(", ", AllowedExtensions)}.");
                return ValidationProblem(ModelState);
            }

            // Spremi na disk pod generiranim imenom (originalno ime ide u metapodatke)
            var storedFileName = $"{Guid.NewGuid():N}{extension}";
            var relativeDir = Path.Combine("uploads", "beers", beerId.ToString());
            var absoluteDir = Path.Combine(_env.WebRootPath, relativeDir);
            Directory.CreateDirectory(absoluteDir);

            var absolutePath = Path.Combine(absoluteDir, storedFileName);
            await using (var stream = System.IO.File.Create(absolutePath))
            {
                await file.CopyToAsync(stream);
            }

            var photo = new BeerPhoto
            {
                BeerId = beerId,
                FileName = Path.GetFileName(file.FileName),
                StoredFileName = storedFileName,
                ContentType = file.ContentType,
                SizeBytes = file.Length,
                RelativePath = Path.Combine(relativeDir, storedFileName).Replace('\\', '/'),
                UploadedAt = DateTime.UtcNow,
                UploadedByUserId = CurrentUserId > 0 ? CurrentUserId : null
            };

            _db.BeerPhotos.Add(photo);
            await _db.SaveChangesAsync();

            await _db.Entry(photo).Reference(p => p.UploadedBy).LoadAsync();

            return CreatedAtAction(nameof(GetForBeer), new { beerId }, photo.ToDto());
        }

        /// <summary>DELETE api/photos/{id} — brisanje datoteke (uploader ili Admin); briše i s diska.</summary>
        [HttpDelete("api/photos/{id:int}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var photo = await _db.BeerPhotos.FindAsync(id);
            if (photo == null)
                return NotFound(new ProblemDetails { Title = $"Datoteka s ID-em {id} ne postoji.", Status = 404 });

            if (photo.UploadedByUserId != CurrentUserId && !User.IsInRole("Admin"))
                return Forbid();

            var absolutePath = Path.Combine(_env.WebRootPath, photo.RelativePath.Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(absolutePath))
                System.IO.File.Delete(absolutePath);

            _db.BeerPhotos.Remove(photo);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
