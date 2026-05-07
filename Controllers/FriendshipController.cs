using Cugger.Models;
using Cugger.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Cugger.Controllers
{
    public class FriendshipController : Controller
    {
        private readonly FriendshipRepository _friendshipRepo;

        public FriendshipController(FriendshipRepository friendshipRepo)
        {
            _friendshipRepo = friendshipRepo;
        }

        public IActionResult Index()
        {
            var friendships = _friendshipRepo.GetAll();
            ViewBag.Breadcrumbs = new[]
            {
                new BreadcrumbItem("Dashboard", "/", false),
                new BreadcrumbItem("Prijateljstva", "/Friendship", true)
            };
            return View(friendships);
        }

        public IActionResult Details(int id)
        {
            var friendship = _friendshipRepo.GetById(id);
            if (friendship == null) return NotFound();

            ViewBag.FromUser = friendship.FromUser;
            ViewBag.ToUser = friendship.ToUser;
            ViewBag.Breadcrumbs = new[]
            {
                new BreadcrumbItem("Dashboard", "/", false),
                new BreadcrumbItem("Prijateljstva", "/Friendship", false),
                new BreadcrumbItem($"Prijateljstvo #{id}", $"/Friendship/Details/{id}", true)
            };

            return View(friendship);
        }
    }
}
