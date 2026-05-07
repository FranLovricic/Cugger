using Cugger.Data;
using Cugger.Models;
using Cugger.Models.ViewModels;
using Cugger.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cugger.Controllers
{
    public class VenueController : Controller
    {
        private readonly CuggerDbContext _db;
        private readonly VenueRepository _venueRepo;
        private readonly CheckInRepository _checkInRepo;

        public VenueController(CuggerDbContext db, VenueRepository venueRepo, CheckInRepository checkInRepo)
        {
            _db = db;
            _venueRepo = venueRepo;
            _checkInRepo = checkInRepo;
        }

        public IActionResult Index()
        {
            var venues = _venueRepo.GetAll();
            ViewBag.Breadcrumbs = new[]
            {
                new BreadcrumbItem("Početna", "/", false),
                new BreadcrumbItem("Lokali", "/Venue", true)
            };
            return View(venues);
        }

        public IActionResult Details(int id)
        {
            var venue = _venueRepo.GetById(id);
            if (venue == null) return NotFound();

            var checkIns = _checkInRepo.GetByVenue(id);
            ViewBag.CheckIns = checkIns;
            ViewBag.CheckInCount = checkIns.Count;
            ViewBag.Breadcrumbs = new[]
            {
                new BreadcrumbItem("Početna", "/", false),
                new BreadcrumbItem("Lokali", "/Venue", false),
                new BreadcrumbItem(venue.Name, $"/Venue/Details/{id}", true)
            };

            return View(venue);
        }

        [HttpGet]
        [Authorize]
        public IActionResult Create()
        {
            ViewBag.Breadcrumbs = new[]
            {
                new BreadcrumbItem("Početna", "/", false),
                new BreadcrumbItem("Lokali", "/Venue", false),
                new BreadcrumbItem("Dodaj lokal", "/Venue/Create", true)
            };
            return View(new CreateVenueViewModel());
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateVenueViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var venue = new Venue
            {
                Name = model.Name.Trim(),
                Address = model.Address.Trim(),
                City = model.City.Trim(),
                Country = model.Country.Trim(),
                Latitude = model.Latitude,
                Longitude = model.Longitude
            };

            _db.Venues.Add(venue);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Dodan lokal: {venue.Name}. 📍";
            return RedirectToAction(nameof(Details), new { id = venue.Id });
        }

        [Route("Venue/City/{city}")]
        public IActionResult ByCity(string city)
        {
            var venues = _venueRepo.GetByCity(city);
            ViewBag.City = city;
            ViewBag.Breadcrumbs = new[]
            {
                new BreadcrumbItem("Početna", "/", false),
                new BreadcrumbItem("Lokali", "/Venue", false),
                new BreadcrumbItem(city, $"/Venue/City/{city}", true)
            };
            return View("Index", venues);
        }
    }
}
