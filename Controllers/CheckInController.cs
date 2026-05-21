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
    public class CheckInController : Controller
    {
        private readonly CuggerDbContext _db;
        private readonly CheckInRepository _checkInRepo;
        private readonly BeerRepository _beerRepo;
        private readonly VenueRepository _venueRepo;

        public CheckInController(
            CuggerDbContext db,
            CheckInRepository checkInRepo,
            BeerRepository beerRepo,
            VenueRepository venueRepo)
        {
            _db = db;
            _checkInRepo = checkInRepo;
            _beerRepo = beerRepo;
            _venueRepo = venueRepo;
        }

        public IActionResult Index()
        {
            var checkIns = _checkInRepo.GetAll();
            ViewBag.Breadcrumbs = new[]
            {
                new BreadcrumbItem("Početna", "/", false),
                new BreadcrumbItem("Check-ini", "/CheckIn", true)
            };
            return View(checkIns);
        }

        public IActionResult Details(int id)
        {
            var checkIn = _checkInRepo.GetById(id);
            if (checkIn == null) return NotFound();

            ViewBag.User = checkIn.User;
            ViewBag.Beer = checkIn.Beer;
            ViewBag.Venue = checkIn.Venue;
            ViewBag.Brewery = checkIn.Beer?.Brewery;
            ViewBag.IsOwner = GetCurrentUserId() == checkIn.UserId;
            ViewBag.Breadcrumbs = new[]
            {
                new BreadcrumbItem("Početna", "/", false),
                new BreadcrumbItem("Check-ini", "/CheckIn", false),
                new BreadcrumbItem($"Check-in #{id}", $"/CheckIn/Details/{id}", true)
            };

            return View(checkIn);
        }

        // ====== Create ======

        [HttpGet]
        [Authorize]
        public IActionResult Create(int? beerId = null)
        {
            ViewBag.Beers = BuildBeerSelect(beerId);
            ViewBag.Venues = BuildVenueSelect();
            ViewBag.Breadcrumbs = new[]
            {
                new BreadcrumbItem("Početna", "/", false),
                new BreadcrumbItem("Feed", "/feed", false),
                new BreadcrumbItem("Novi check-in", "/CheckIn/Create", true)
            };

            var model = new CreateCheckInViewModel
            {
                PrefilledBeerId = beerId,
                BeerId = beerId ?? 0
            };
            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCheckInViewModel model)
        {
            if (!await _db.Beers.AnyAsync(b => b.Id == model.BeerId))
                ModelState.AddModelError(nameof(model.BeerId), "Odaberi postojeće pivo.");

            if (!await _db.Venues.AnyAsync(v => v.Id == model.VenueId))
                ModelState.AddModelError(nameof(model.VenueId), "Odaberi postojeći lokal.");

            if (!ModelState.IsValid)
            {
                ViewBag.Beers = BuildBeerSelect(model.BeerId);
                ViewBag.Venues = BuildVenueSelect(model.VenueId);
                return View(model);
            }

            var userId = GetCurrentUserId();
            if (userId == null) return Forbid();

            var checkIn = new CheckIn
            {
                UserId = userId.Value,
                BeerId = model.BeerId,
                VenueId = model.VenueId,
                Rating = model.Rating,
                Comment = (model.Comment ?? string.Empty).Trim(),
                CheckInDate = model.CheckInDate.Date,
                CreatedAt = DateTime.UtcNow
            };

            _db.CheckIns.Add(checkIn);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Check-in spremljen. Na zdravlje! 🍻";
            return RedirectToAction(nameof(Details), new { id = checkIn.Id });
        }

        // ====== Edit ======

        [HttpGet]
        [Authorize]
        public IActionResult Edit(int id)
        {
            var checkIn = _checkInRepo.GetById(id);
            if (checkIn == null) return NotFound();

            var userId = GetCurrentUserId();
            if (userId == null || checkIn.UserId != userId.Value)
                return Forbid();

            var model = new CreateCheckInViewModel
            {
                BeerId = checkIn.BeerId,
                VenueId = checkIn.VenueId,
                Rating = checkIn.Rating,
                Comment = checkIn.Comment,
                CheckInDate = checkIn.CheckInDate
            };

            ViewBag.CheckInId = id;
            ViewBag.Beers = BuildBeerSelect(checkIn.BeerId);
            ViewBag.Venues = BuildVenueSelect(checkIn.VenueId);
            ViewBag.Breadcrumbs = new[]
            {
                new BreadcrumbItem("Početna", "/", false),
                new BreadcrumbItem("Feed", "/feed", false),
                new BreadcrumbItem($"Check-in #{id}", $"/CheckIn/Details/{id}", false),
                new BreadcrumbItem("Uredi", $"/CheckIn/Edit/{id}", true)
            };

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreateCheckInViewModel model)
        {
            var checkIn = await _db.CheckIns.FindAsync(id);
            if (checkIn == null) return NotFound();

            var userId = GetCurrentUserId();
            if (userId == null || checkIn.UserId != userId.Value)
                return Forbid();

            if (!await _db.Beers.AnyAsync(b => b.Id == model.BeerId))
                ModelState.AddModelError(nameof(model.BeerId), "Odaberi postojeće pivo.");
            if (!await _db.Venues.AnyAsync(v => v.Id == model.VenueId))
                ModelState.AddModelError(nameof(model.VenueId), "Odaberi postojeći lokal.");

            if (!ModelState.IsValid)
            {
                ViewBag.CheckInId = id;
                ViewBag.Beers = BuildBeerSelect(model.BeerId);
                ViewBag.Venues = BuildVenueSelect(model.VenueId);
                return View(model);
            }

            checkIn.BeerId = model.BeerId;
            checkIn.VenueId = model.VenueId;
            checkIn.Rating = model.Rating;
            checkIn.Comment = (model.Comment ?? string.Empty).Trim();
            checkIn.CheckInDate = model.CheckInDate.Date;

            await _db.SaveChangesAsync();

            TempData["Success"] = "Check-in izmijenjen. 🍻";
            return RedirectToAction(nameof(Details), new { id = checkIn.Id });
        }

        // ====== Delete ======

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var checkIn = await _db.CheckIns.FindAsync(id);
            if (checkIn == null) return NotFound();

            var userId = GetCurrentUserId();
            if (userId == null || checkIn.UserId != userId.Value)
                return Forbid();

            _db.CheckIns.Remove(checkIn);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Check-in obrisan.";
            return RedirectToAction(nameof(Index));
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

        private List<SelectListItem> BuildVenueSelect(int? selected = null)
        {
            return _venueRepo.GetAll()
                .Select(v => new SelectListItem
                {
                    Value = v.Id.ToString(),
                    Text = $"{v.Name} ({v.City})",
                    Selected = selected.HasValue && selected.Value == v.Id
                })
                .ToList();
        }
    }
}
