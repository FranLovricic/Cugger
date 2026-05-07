using Cugger.Models;
using Cugger.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Cugger.Controllers
{
    public class UserController : Controller
    {
        private readonly UserRepository _userRepo;
        private readonly CheckInRepository _checkInRepo;
        private readonly ReviewRepository _reviewRepo;

        public UserController(
            UserRepository userRepo,
            CheckInRepository checkInRepo,
            ReviewRepository reviewRepo)
        {
            _userRepo = userRepo;
            _checkInRepo = checkInRepo;
            _reviewRepo = reviewRepo;
        }

        public IActionResult Index()
        {
            var users = _userRepo.GetAll();
            ViewBag.Breadcrumbs = new[]
            {
                new BreadcrumbItem("Dashboard", "/", false),
                new BreadcrumbItem("Korisnici", "/User", true)
            };
            return View(users);
        }

        public IActionResult Details(int id)
        {
            var user = _userRepo.GetById(id);
            if (user == null) return NotFound();

            ViewBag.CheckIns = _checkInRepo.GetByUser(id);
            ViewBag.Friends = _userRepo.GetFriends(id);
            ViewBag.Reviews = _reviewRepo.GetByUser(id);
            ViewBag.CheckInCount = _userRepo.GetCheckInCount(id);
            ViewBag.FriendsCount = _userRepo.GetFriendsCount(id);
            ViewBag.Breadcrumbs = new[]
            {
                new BreadcrumbItem("Dashboard", "/", false),
                new BreadcrumbItem("Korisnici", "/User", false),
                new BreadcrumbItem(user.FirstName + " " + user.LastName, $"/User/Details/{id}", true)
            };

            return View(user);
        }

        // /korisnik/{username} — pretty URL custom routing (mapirano u Program.cs)
        public IActionResult ByUsername(string username)
        {
            if (string.IsNullOrEmpty(username)) return NotFound();

            var user = _userRepo.GetByUsername(username);
            if (user == null) return NotFound();

            return RedirectToAction(nameof(Details), new { id = user.Id });
        }
    }
}
