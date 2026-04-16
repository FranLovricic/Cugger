using Cugger.Models;
using Cugger.Services;
using Microsoft.AspNetCore.Mvc;

namespace Cugger.Controllers
{
    public class VenueController : Controller
    {
        private readonly CuggerDataService _dataService;

        public VenueController(CuggerDataService dataService)
        {
            _dataService = dataService;
        }

        public IActionResult Index()
        {
            var venues = _dataService.GetAllVenues();
            ViewBag.Breadcrumbs = new[] {
                new BreadcrumbItem("Dashboard", "/", false),
                new BreadcrumbItem("Lokale", "/Venue", true)
            };
            return View(venues);
        }

        public IActionResult Details(int id)
        {
            var venue = _dataService.GetVenueById(id);
            if (venue == null)
                return NotFound();

            var checkIns = _dataService.GetCheckInsByVenue(id);
            ViewBag.CheckIns = checkIns;
            ViewBag.CheckInCount = checkIns.Count;
            ViewBag.Breadcrumbs = new[] {
                new BreadcrumbItem("Dashboard", "/", false),
                new BreadcrumbItem("Lokale", "/Venue", false),
                new BreadcrumbItem(venue.Name, $"/Venue/Details/{id}", true)
            };

            return View(venue);
        }
    }
}
