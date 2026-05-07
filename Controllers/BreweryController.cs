using Cugger.Data;
using Cugger.Models;
using Cugger.Models.ViewModels;
using Cugger.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        [Authorize]
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
        [Authorize]
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
