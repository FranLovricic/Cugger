using Cugger.Models;
using Cugger.Services;
using Microsoft.AspNetCore.Mvc;

namespace Cugger.Controllers
{
    public class BeerController : Controller
    {
        private readonly CuggerDataService _dataService;

        public BeerController(CuggerDataService dataService)
        {
            _dataService = dataService;
        }

        public IActionResult Index()
        {
            var beers = _dataService.GetAllBeers();
            ViewBag.Breadcrumbs = new[] {
                new BreadcrumbItem("Dashboard", "/", false),
                new BreadcrumbItem("Piva", "/Beer", true)
            };
            return View(beers);
        }

        public IActionResult Details(int id)
        {
            var beer = _dataService.GetBeerById(id);
            if (beer == null)
                return NotFound();

            ViewBag.Brewery = _dataService.GetBreweryById(beer.BreweryId);
            ViewBag.CheckIns = _dataService.GetCheckInsByBeer(id);
            ViewBag.Reviews = _dataService.GetReviewsByBeer(id);
            ViewBag.AverageRating = _dataService.GetBeerAverageRating(id);
            ViewBag.RatingCount = _dataService.GetCheckInsByBeer(id).Count;
            ViewBag.Breadcrumbs = new[] {
                new BreadcrumbItem("Dashboard", "/", false),
                new BreadcrumbItem("Piva", "/Beer", false),
                new BreadcrumbItem(beer.Name, $"/Beer/Details/{id}", true)
            };

            return View(beer);
        }
    }
}
