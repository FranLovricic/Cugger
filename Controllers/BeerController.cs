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
    public class BeerController : Controller
    {
        private readonly CuggerDbContext _db;
        private readonly BeerRepository _beerRepo;
        private readonly BreweryRepository _breweryRepo;
        private readonly CheckInRepository _checkInRepo;
        private readonly ReviewRepository _reviewRepo;

        public BeerController(
            CuggerDbContext db,
            BeerRepository beerRepo,
            BreweryRepository breweryRepo,
            CheckInRepository checkInRepo,
            ReviewRepository reviewRepo)
        {
            _db = db;
            _beerRepo = beerRepo;
            _breweryRepo = breweryRepo;
            _checkInRepo = checkInRepo;
            _reviewRepo = reviewRepo;
        }

        public IActionResult Index(string? style = null)
        {
            var beers = string.IsNullOrEmpty(style) || !Enum.TryParse<BeerStyle>(style, true, out var parsed)
                ? _beerRepo.GetAll()
                : _beerRepo.GetByStyle(parsed);

            foreach (var b in beers)
            {
                b.AverageRating = _beerRepo.GetAverageRating(b.Id);
                b.RatingCount = _beerRepo.GetRatingCount(b.Id);
            }

            ViewBag.SelectedStyle = style;
            ViewBag.Breadcrumbs = new[]
            {
                new BreadcrumbItem("Početna", "/", false),
                new BreadcrumbItem("Piva", "/Beer", true)
            };
            return View(beers);
        }

        public IActionResult Details(int id)
        {
            var beer = _beerRepo.GetById(id);
            if (beer == null) return NotFound();

            ViewBag.Brewery = beer.Brewery;
            ViewBag.CheckIns = _checkInRepo.GetByBeer(id);
            ViewBag.Reviews = _reviewRepo.GetByBeer(id);
            ViewBag.AverageRating = _beerRepo.GetAverageRating(id);
            ViewBag.RatingCount = _beerRepo.GetRatingCount(id);
            ViewBag.Breadcrumbs = new[]
            {
                new BreadcrumbItem("Početna", "/", false),
                new BreadcrumbItem("Piva", "/Beer", false),
                new BreadcrumbItem(beer.Name, $"/Beer/Details/{id}", true)
            };

            return View(beer);
        }

        // ====== Create ======

        [HttpGet]
        [Authorize]
        public IActionResult Create()
        {
            ViewBag.Breweries = BuildBrewerySelect();
            ViewBag.Breadcrumbs = new[]
            {
                new BreadcrumbItem("Početna", "/", false),
                new BreadcrumbItem("Piva", "/Beer", false),
                new BreadcrumbItem("Dodaj pivo", "/Beer/Create", true)
            };
            return View(new CreateBeerViewModel());
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateBeerViewModel model)
        {
            if (!await _db.Breweries.AnyAsync(b => b.Id == model.BreweryId))
                ModelState.AddModelError(nameof(model.BreweryId), "Odaberi postojeću pivovaru.");

            if (!ModelState.IsValid)
            {
                ViewBag.Breweries = BuildBrewerySelect(model.BreweryId);
                return View(model);
            }

            var beer = new Beer
            {
                Name = model.Name.Trim(),
                BreweryId = model.BreweryId,
                Style = model.Style,
                ABV = model.ABV,
                IBU = model.IBU,
                Description = (model.Description ?? string.Empty).Trim(),
                ImageUrl = (model.ImageUrl ?? string.Empty).Trim()
            };

            _db.Beers.Add(beer);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Dodano: {beer.Name}. 🍺";
            return RedirectToAction(nameof(Details), new { id = beer.Id });
        }

        // ====== Edit ======

        [HttpGet]
        [Authorize]
        public IActionResult Edit(int id)
        {
            var beer = _beerRepo.GetById(id);
            if (beer == null) return NotFound();

            var model = new CreateBeerViewModel
            {
                Name = beer.Name,
                BreweryId = beer.BreweryId,
                Style = beer.Style,
                ABV = beer.ABV,
                IBU = beer.IBU,
                Description = beer.Description,
                ImageUrl = beer.ImageUrl
            };

            ViewBag.BeerId = id;
            ViewBag.Breweries = BuildBrewerySelect(beer.BreweryId);
            ViewBag.Breadcrumbs = new[]
            {
                new BreadcrumbItem("Početna", "/", false),
                new BreadcrumbItem("Piva", "/Beer", false),
                new BreadcrumbItem(beer.Name, $"/pivo/{id}", false),
                new BreadcrumbItem("Uredi", $"/Beer/Edit/{id}", true)
            };

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreateBeerViewModel model)
        {
            var beer = await _db.Beers.FindAsync(id);
            if (beer == null) return NotFound();

            if (!await _db.Breweries.AnyAsync(b => b.Id == model.BreweryId))
                ModelState.AddModelError(nameof(model.BreweryId), "Odaberi postojeću pivovaru.");

            if (!ModelState.IsValid)
            {
                ViewBag.BeerId = id;
                ViewBag.Breweries = BuildBrewerySelect(model.BreweryId);
                return View(model);
            }

            beer.Name = model.Name.Trim();
            beer.BreweryId = model.BreweryId;
            beer.Style = model.Style;
            beer.ABV = model.ABV;
            beer.IBU = model.IBU;
            beer.Description = (model.Description ?? string.Empty).Trim();
            beer.ImageUrl = (model.ImageUrl ?? string.Empty).Trim();

            await _db.SaveChangesAsync();

            TempData["Success"] = $"Spremljene izmjene za '{beer.Name}'. 🍺";
            return RedirectToAction(nameof(Details), new { id = beer.Id });
        }

        // ====== Delete ======

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var beer = await _db.Beers
                .Include(b => b.CheckIns)
                .Include(b => b.Reviews)
                .FirstOrDefaultAsync(b => b.Id == id);
            if (beer == null) return NotFound();

            if (beer.CheckIns.Any() || beer.Reviews.Any())
            {
                TempData["Error"] = $"Pivo '{beer.Name}' ne možeš obrisati — već postoje check-ini ili recenzije.";
                return RedirectToAction(nameof(Details), new { id });
            }

            _db.Beers.Remove(beer);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Obrisano pivo '{beer.Name}'.";
            return RedirectToAction(nameof(Index));
        }

        // ====== Custom routing ======

        [Route("pretraga")]
        public IActionResult Search(string? q)
        {
            var results = _beerRepo.Search(q);
            foreach (var b in results)
            {
                b.AverageRating = _beerRepo.GetAverageRating(b.Id);
                b.RatingCount = _beerRepo.GetRatingCount(b.Id);
            }
            ViewBag.Query = q;
            ViewBag.Breadcrumbs = new[]
            {
                new BreadcrumbItem("Početna", "/", false),
                new BreadcrumbItem("Pretraga", "/pretraga", true)
            };
            return View(results);
        }

        [Route("Beer/Style/{style}")]
        public IActionResult Style(string style)
        {
            if (!Enum.TryParse<BeerStyle>(style, true, out var parsed))
                return NotFound();

            var beers = _beerRepo.GetByStyle(parsed);
            foreach (var b in beers)
            {
                b.AverageRating = _beerRepo.GetAverageRating(b.Id);
                b.RatingCount = _beerRepo.GetRatingCount(b.Id);
            }

            ViewBag.SelectedStyle = parsed.ToString();
            ViewBag.Breadcrumbs = new[]
            {
                new BreadcrumbItem("Početna", "/", false),
                new BreadcrumbItem("Piva", "/Beer", false),
                new BreadcrumbItem(parsed.ToString(), $"/Beer/Style/{parsed}", true)
            };
            return View("Index", beers);
        }

        // ====== Helpers ======

        private List<SelectListItem> BuildBrewerySelect(int? selected = null)
        {
            return _breweryRepo.GetAll()
                .Select(b => new SelectListItem
                {
                    Value = b.Id.ToString(),
                    Text = $"{b.Name} ({b.City}, {b.Country})",
                    Selected = selected.HasValue && selected.Value == b.Id
                })
                .ToList();
        }
    }
}
