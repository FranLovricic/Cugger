using Cugger.Models;
using Cugger.Services;
using Microsoft.AspNetCore.Mvc;

namespace Cugger.Controllers
{
    public class BreweryController : Controller
    {
        private readonly CuggerDataService _dataService;

        public BreweryController(CuggerDataService dataService)
        {
            _dataService = dataService;
        }

        public IActionResult Index()
        {
            var breweries = _dataService.GetAllBreweries();
            ViewBag.Breadcrumbs = new[] {
                new BreadcrumbItem("Dashboard", "/", false),
                new BreadcrumbItem("Pivovare", "/Brewery", true)
            };
            return View(breweries);
        }

        public IActionResult Details(int id)
        {
            var brewery = _dataService.GetBreweryById(id);
            if (brewery == null)
                return NotFound();

            var beers = _dataService.GetAllBeers().Where(b => b.BreweryId == id).ToList();
            ViewBag.Beers = beers;
            ViewBag.Breadcrumbs = new[] {
                new BreadcrumbItem("Dashboard", "/", false),
                new BreadcrumbItem("Pivovare", "/Brewery", false),
                new BreadcrumbItem(brewery.Name, $"/Brewery/Details/{id}", true)
            };

            return View(brewery);
        }
    }
}
