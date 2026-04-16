using Cugger.Models;
using Cugger.Services;
using Microsoft.AspNetCore.Mvc;

namespace Cugger.Controllers
{
    public class CheckInController : Controller
    {
        private readonly CuggerDataService _dataService;

        public CheckInController(CuggerDataService dataService)
        {
            _dataService = dataService;
        }

        public IActionResult Index()
        {
            var checkIns = _dataService.GetAllCheckIns();
            ViewBag.Breadcrumbs = new[] {
                new BreadcrumbItem("Dashboard", "/", false),
                new BreadcrumbItem("Check-ini", "/CheckIn", true)
            };
            return View(checkIns);
        }

        public IActionResult Details(int id)
        {
            var checkIn = _dataService.GetCheckInById(id);
            if (checkIn == null)
                return NotFound();

            ViewBag.User = _dataService.GetUserById(checkIn.UserId);
            ViewBag.Beer = _dataService.GetBeerById(checkIn.BeerId);
            ViewBag.Venue = _dataService.GetVenueById(checkIn.VenueId);
            ViewBag.Brewery = _dataService.GetBreweryById(_dataService.GetBeerById(checkIn.BeerId)?.BreweryId ?? 0);
            ViewBag.Breadcrumbs = new[] {
                new BreadcrumbItem("Dashboard", "/", false),
                new BreadcrumbItem("Check-ini", "/CheckIn", false),
                new BreadcrumbItem($"Check-in #{id}", $"/CheckIn/Details/{id}", true)
            };

            return View(checkIn);
        }
    }
}
