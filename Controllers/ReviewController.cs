using System.Security.Claims;
using Cugger.Data;
using Cugger.Models;
using Cugger.Models.ViewModels;
using Cugger.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Cugger.Controllers
{
    public class ReviewController : Controller
    {
        private readonly CuggerDbContext _db;
        private readonly ReviewRepository _reviewRepo;
        private readonly BeerRepository _beerRepo;

        public ReviewController(
            CuggerDbContext db,
            ReviewRepository reviewRepo,
            BeerRepository beerRepo)
        {
            _db = db;
            _reviewRepo = reviewRepo;
            _beerRepo = beerRepo;
        }

        public IActionResult Index()
        {
            var reviews = _reviewRepo.GetAll();
            ViewBag.Breadcrumbs = new[]
            {
                new BreadcrumbItem("Početna", "/", false),
                new BreadcrumbItem("Recenzije", "/Review", true)
            };
            return View(reviews);
        }

        public IActionResult Details(int id)
        {
            var review = _reviewRepo.GetById(id);
            if (review == null) return NotFound();

            ViewBag.User = review.User;
            ViewBag.Beer = review.Beer;
            ViewBag.Brewery = review.Beer?.Brewery;
            ViewBag.IsOwner = GetCurrentUserId() == review.UserId;
            ViewBag.Breadcrumbs = new[]
            {
                new BreadcrumbItem("Početna", "/", false),
                new BreadcrumbItem("Recenzije", "/Review", false),
                new BreadcrumbItem($"Recenzija #{id}", $"/Review/Details/{id}", true)
            };

            return View(review);
        }

        // ====== Create ======

        [HttpGet]
        [Authorize]
        public IActionResult Create(int? beerId = null)
        {
            ViewBag.Beers = BuildBeerSelect(beerId);
            ViewBag.Breadcrumbs = new[]
            {
                new BreadcrumbItem("Početna", "/", false),
                new BreadcrumbItem("Recenzije", "/Review", false),
                new BreadcrumbItem("Nova recenzija", "/Review/Create", true)
            };

            return View(new CreateReviewViewModel
            {
                BeerId = beerId ?? 0
            });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateReviewViewModel model)
        {
            if (!await _db.Beers.AnyAsync(b => b.Id == model.BeerId))
                ModelState.AddModelError(nameof(model.BeerId), "Odaberi postojeće pivo.");

            if (!ModelState.IsValid)
            {
                ViewBag.Beers = BuildBeerSelect(model.BeerId);
                return View(model);
            }

            var userId = GetCurrentUserId();
            if (userId == null) return Forbid();

            var review = new Review
            {
                UserId = userId.Value,
                BeerId = model.BeerId,
                Rating = model.Rating,
                Comment = model.Comment.Trim(),
                CreatedAt = DateTime.UtcNow,
                Likes = 0
            };

            _db.Reviews.Add(review);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Recenzija objavljena. 📝";
            return RedirectToAction(nameof(Details), new { id = review.Id });
        }

        // ====== Like ======

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Like(int id, string? returnUrl = null)
        {
            var review = await _db.Reviews.FindAsync(id);
            if (review == null) return NotFound();

            review.Likes += 1;
            await _db.SaveChangesAsync();

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction(nameof(Details), new { id });
        }

        // ====== Delete ======

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var review = await _db.Reviews.FindAsync(id);
            if (review == null) return NotFound();

            var userId = GetCurrentUserId();
            if (userId == null || review.UserId != userId.Value)
                return Forbid();

            _db.Reviews.Remove(review);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Recenzija obrisana.";
            return RedirectToAction(nameof(Index));
        }

        // ====== Custom routing ======

        [Route("Review/Top")]
        public IActionResult Top()
        {
            var topReviews = _reviewRepo.GetTopLiked(10);
            ViewBag.Breadcrumbs = new[]
            {
                new BreadcrumbItem("Početna", "/", false),
                new BreadcrumbItem("Recenzije", "/Review", false),
                new BreadcrumbItem("Top", "/Review/Top", true)
            };
            return View("Index", topReviews);
        }

        // ====== Helpers ======

        private int? GetCurrentUserId()
        {
            var v = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(v, out var id) ? id : null;
        }

        private List<SelectListItem> BuildBeerSelect(int? selected = null)
        {
            return _beerRepo.GetAll()
                .Select(b => new SelectListItem
                {
                    Value = b.Id.ToString(),
                    Text = b.Brewery != null ? $"{b.Name} — {b.Brewery.Name}" : b.Name,
                    Selected = selected.HasValue && selected.Value == b.Id
                })
                .ToList();
        }
    }
}
