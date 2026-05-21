using Cugger.Data;
using Cugger.Models;
using Cugger.Models.ViewModels;
using Cugger.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        // ====== Edit ======

        [HttpGet]
        [Authorize]
        public IActionResult Edit(int id)
        {
            var venue = _venueRepo.GetById(id);
            if (venue == null) return NotFound();

            var model = new CreateVenueViewModel
            {
                Name = venue.Name,
                Address = venue.Address,
                City = venue.City,
                Country = venue.Country,
                Latitude = venue.Latitude,
                Longitude = venue.Longitude
            };

            ViewBag.VenueId = id;
            ViewBag.Breadcrumbs = new[]
            {
                new BreadcrumbItem("Početna", "/", false),
                new BreadcrumbItem("Lokali", "/Venue", false),
                new BreadcrumbItem(venue.Name, $"/Venue/Details/{id}", false),
                new BreadcrumbItem("Uredi", $"/Venue/Edit/{id}", true)
            };

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreateVenueViewModel model)
        {
            var venue = await _db.Venues.FindAsync(id);
            if (venue == null) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.VenueId = id;
                return View(model);
            }

            venue.Name = model.Name.Trim();
            venue.Address = model.Address.Trim();
            venue.City = model.City.Trim();
            venue.Country = model.Country.Trim();
            venue.Latitude = model.Latitude;
            venue.Longitude = model.Longitude;

            await _db.SaveChangesAsync();

            TempData["Success"] = $"Spremljene izmjene za '{venue.Name}'. 📍";
            return RedirectToAction(nameof(Details), new { id = venue.Id });
        }

        // ====== Delete ======

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var venue = await _db.Venues
                .Include(v => v.CheckIns)
                .FirstOrDefaultAsync(v => v.Id == id);
            if (venue == null) return NotFound();

            if (venue.CheckIns.Any())
            {
                TempData["Error"] = $"Lokal '{venue.Name}' ne možeš obrisati — već postoje check-ini na njemu.";
                return RedirectToAction(nameof(Details), new { id });
            }

            _db.Venues.Remove(venue);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Obrisan lokal '{venue.Name}'.";
            return RedirectToAction(nameof(Index));
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
