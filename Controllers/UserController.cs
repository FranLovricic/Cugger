using Cugger.Models;
using Cugger.Services;
using Microsoft.AspNetCore.Mvc;

namespace Cugger.Controllers
{
    public class UserController : Controller
    {
        private readonly CuggerDataService _dataService;

        public UserController(CuggerDataService dataService)
        {
            _dataService = dataService;
        }

        public IActionResult Index()
        {
            var users = _dataService.GetAllUsers();
            ViewBag.Breadcrumbs = new[] {
                new BreadcrumbItem("Dashboard", "/", false),
                new BreadcrumbItem("Korisnici", "/User", true)
            };
            return View(users);
        }

        public IActionResult Details(int id)
        {
            var user = _dataService.GetUserById(id);
            if (user == null)
                return NotFound();

            ViewBag.CheckIns = _dataService.GetCheckInsByUser(id);
            ViewBag.Friends = _dataService.GetUserFriends(id);
            ViewBag.Reviews = _dataService.GetReviewsByUser(id);
            ViewBag.CheckInCount = _dataService.GetUserCheckInCount(id);
            ViewBag.FriendsCount = _dataService.GetUserFriendsCount(id);
            ViewBag.Breadcrumbs = new[] {
                new BreadcrumbItem("Dashboard", "/", false),
                new BreadcrumbItem("Korisnici", "/User", false),
                new BreadcrumbItem(user.FirstName + " " + user.LastName, $"/User/Details/{id}", true)
            };

            return View(user);
        }
    }
}
