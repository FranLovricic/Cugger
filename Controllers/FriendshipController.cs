using Cugger.Models;
using Cugger.Services;
using Microsoft.AspNetCore.Mvc;

namespace Cugger.Controllers
{
    public class FriendshipController : Controller
    {
        private readonly CuggerDataService _dataService;

        public FriendshipController(CuggerDataService dataService)
        {
            _dataService = dataService;
        }

        public IActionResult Index()
        {
            var friendships = _dataService.GetAllFriendships();
            ViewBag.Breadcrumbs = new[] {
                new BreadcrumbItem("Dashboard", "/", false),
                new BreadcrumbItem("Prijateljstva", "/Friendship", true)
            };
            return View(friendships);
        }

        public IActionResult Details(int id)
        {
            var friendship = _dataService.GetFriendshipById(id);
            if (friendship == null)
                return NotFound();

            ViewBag.FromUser = _dataService.GetUserById(friendship.FromUserId);
            ViewBag.ToUser = _dataService.GetUserById(friendship.ToUserId);
            ViewBag.Breadcrumbs = new[] {
                new BreadcrumbItem("Dashboard", "/", false),
                new BreadcrumbItem("Prijateljstva", "/Friendship", false),
                new BreadcrumbItem($"Prijateljstvo #{id}", $"/Friendship/Details/{id}", true)
            };

            return View(friendship);
        }
    }
}
