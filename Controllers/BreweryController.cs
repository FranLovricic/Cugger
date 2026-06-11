using Cugger.Data;
using Cugger.Models;
using Cugger.Models.ViewModels;
using Cugger.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cugger.Controllers
{
    public class BreweryController : Controller
    {
        private readonly CuggerDbContext _db;
        private readonly BreweryRepository _breweryRepo;
        private readonly BeerRepository _beerRepo;

        public BreweryController(CuggerDbContext db, BreweryRepository breweryRepo, BeerRepository beerRepo)
        {
            _db = db;
            _breweryRepo = breweryRepo;
            _beerRepo = beerRepo;
        }

        public IActionResult Index()
        {
            var breweries = _breweryRepo.GetAll();
            ViewBag.Breadcrumbs = new[]
            {
                new BreadcrumbItem("Početna", "/", false),
                new BreadcrumbItem("Pivovare", "/Brewery", true)
            };
            return View(breweries);
        }

        public IActionResult Details(int id)
        {
            var brewery = _breweryRepo.GetById(id);
            if (brewery == null) return NotFound();

            var beers = _beerRepo.GetByBrewery(id);
            foreach (var b in beers)
            {
                b.AverageRating = _beerRepo.GetAverageRating(b.Id);
                b.RatingCount = _beerRepo.GetRatingCount(b.Id);
            }
            ViewBag.Beers = beers;
            ViewBag.Breadcrumbs = new[]
            {
                new BreadcrumbItem("Početna", "/", false),
                new BreadcrumbItem("Pivovare", "/Brewery", false),
                new BreadcrumbItem(brewery.Name, $"/Brewery/Details/{id}", true)
            };

            return View(brewery);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewBag.Breadcrumbs = new[]
            {
                new BreadcrumbItem("Početna", "/", false),
                new BreadcrumbItem("Pivovare", "/Brewery", false),
                new BreadcrumbItem("Dodaj pivovaru", "/Brewery/Create", true)
            };
            return View(new CreateBreweryViewModel());
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateBreweryViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var brewery = new Brewery
            {
                Name = model.Name.Trim(),
                Country = model.Country.Trim(),
                City = model.City.Trim(),
                FoundedYear = model.FoundedYear,
                Description = (model.Description ?? string.Empty).Trim(),
                WebsiteUrl = (model.WebsiteUrl ?? string.Empty).Trim(),
                LogoUrl = string.Empty
            };

            _db.Breweries.Add(brewery);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Dodana pivovara: {brewery.Name}. 🏭";
            return RedirectToAction(nameof(Details), new { id = brewery.Id });
        }

        // ====== Edit ======

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Edit(int id)
        {
            var brewery = _breweryRepo.GetById(id);
            if (brewery == null) return NotFound();

            var model = new CreateBreweryViewModel
            {
                Name = brewery.Name,
                Country = brewery.Country,
                City = brewery.City,
                FoundedYear = brewery.FoundedYear,
                Description = brewery.Description,
                WebsiteUrl = brewery.WebsiteUrl
            };

            ViewBag.BreweryId = id;
            ViewBag.Breadcrumbs = new[]
            {
                new BreadcrumbItem("Početna", "/", false),
                new BreadcrumbItem("Pivovare", "/Brewery", false),
                new BreadcrumbItem(brewery.Name, $"/pivovara/{id}", false),
                new BreadcrumbItem("Uredi", $"/Brewery/Edit/{id}", true)
            };

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreateBreweryViewModel model)
        {
            var brewery = await _db.Breweries.FindAsync(id);
            if (brewery == null) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.BreweryId = id;
                return View(model);
            }

            brewery.Name = model.Name.Trim();
            brewery.Country = model.Country.Trim();
            brewery.City = model.City.Trim();
            brewery.FoundedYear = model.FoundedYear;
            brewery.Description = (model.Description ?? string.Empty).Trim();
            brewery.WebsiteUrl = (model.WebsiteUrl ?? string.Empty).Trim();

            await _db.SaveChangesAsync();

            TempData["Success"] = $"Spremljene izmjene za '{brewery.Name}'. 🏭";
            return RedirectToAction(nameof(Details), new { id = brewery.Id });
        }

        // ====== Delete ======

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var brewery = await _db.Breweries
                .Include(b => b.Beers)
                .FirstOrDefaultAsync(b => b.Id == id);
            if (brewery == null) return NotFound();

            if (brewery.Beers.Any())
            {
                TempData["Error"] = $"Pivovaru '{brewery.Name}' ne možeš obrisati — još uvijek ima piva u katalogu.";
                return RedirectToAction(nameof(Details), new { id });
            }

            _db.Breweries.Remove(brewery);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Obrisana pivovara '{brewery.Name}'.";
            return RedirectToAction(nameof(Index));
        }

        [Route("Brewery/Country/{country}")]
        public IActionResult ByCountry(string country)
        {
            var breweries = _breweryRepo.GetByCountry(country);
            ViewBag.Country = country;
            ViewBag.Breadcrumbs = new[]
            {
                new BreadcrumbItem("Početna", "/", false),
                new BreadcrumbItem("Pivovare", "/Brewery", false),
                new BreadcrumbItem(country, $"/Brewery/Country/{country}", true)
            };
            return View("Index", breweries);
        }
    }
}
