using Cugger.Models;
using Cugger.Services;
using Microsoft.AspNetCore.Mvc;

namespace Cugger.Controllers
{
    public class ReviewController : Controller
    {
        private readonly CuggerDataService _dataService;

        public ReviewController(CuggerDataService dataService)
        {
            _dataService = dataService;
        }

        public IActionResult Index()
        {
            var reviews = _dataService.GetAllReviews();
            ViewBag.Breadcrumbs = new[] {
                new BreadcrumbItem("Dashboard", "/", false),
                new BreadcrumbItem("Recenzije", "/Review", true)
            };
            return View(reviews);
        }

        public IActionResult Details(int id)
        {
            var review = _dataService.GetReviewById(id);
            if (review == null)
                return NotFound();

            ViewBag.User = _dataService.GetUserById(review.UserId);
            ViewBag.Beer = _dataService.GetBeerById(review.BeerId);
            ViewBag.Brewery = _dataService.GetBreweryById(_dataService.GetBeerById(review.BeerId)?.BreweryId ?? 0);
            ViewBag.Breadcrumbs = new[] {
                new BreadcrumbItem("Dashboard", "/", false),
                new BreadcrumbItem("Recenzije", "/Review", false),
                new BreadcrumbItem($"Recenzija #{id}", $"/Review/Details/{id}", true)
            };

            return View(review);
        }
    }
}
