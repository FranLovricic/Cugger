using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Cugger.Models;
using Cugger.Repositories;

namespace Cugger.Controllers;

public class HomeController : Controller
{
    private readonly BeerRepository _beerRepo;
    private readonly UserRepository _userRepo;
    private readonly CheckInRepository _checkInRepo;
    private readonly BreweryRepository _breweryRepo;

    public HomeController(
        BeerRepository beerRepo,
        UserRepository userRepo,
        CheckInRepository checkInRepo,
        BreweryRepository breweryRepo)
    {
        _beerRepo = beerRepo;
        _userRepo = userRepo;
        _checkInRepo = checkInRepo;
        _breweryRepo = breweryRepo;
    }

    public IActionResult Index()
    {
        ViewBag.RecentCheckIns = _checkInRepo.GetRecent(6);
        ViewBag.TopRatedBeers = _beerRepo.GetTopRated(8);
        ViewBag.MostActiveUsers = _userRepo.GetMostActive(5);
        ViewBag.TotalUsers = _userRepo.GetAll().Count;
        ViewBag.TotalBeers = _beerRepo.GetAll().Count;
        ViewBag.TotalCheckIns = _checkInRepo.GetAll().Count;
        ViewBag.TotalBreweries = _breweryRepo.GetAll().Count;

        return View();
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
