using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Cugger.Models;
using Cugger.Services;

namespace Cugger.Controllers;

public class HomeController : Controller
{
    private readonly CuggerDataService _dataService;

    public HomeController(CuggerDataService dataService)
    {
        _dataService = dataService;
    }

    public IActionResult Index()
    {
        ViewBag.RecentCheckIns = _dataService.GetTopRecentCheckIns(6);
        ViewBag.TopRatedBeers = _dataService.GetTopRatedBeers(5);
        ViewBag.MostActiveUsers = _dataService.GetMostActiveUsers(5);
        ViewBag.TotalUsers = _dataService.GetAllUsers().Count;
        ViewBag.TotalBeers = _dataService.GetAllBeers().Count;
        ViewBag.TotalCheckIns = _dataService.GetAllCheckIns().Count;

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
