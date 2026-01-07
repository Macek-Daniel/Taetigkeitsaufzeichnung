using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; // <--- WICHTIG: Diesen Namespace hinzufügen
using Taetigkeitsaufzeichnung.Models;

namespace Taetigkeitsaufzeichnung.Controllers;

[Authorize] // <--- WICHTIG: Das erzwingt den Login für alle Aktionen in diesem Controller
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [AllowAnonymous] // <--- Damit Fehler auch ohne Login angezeigt werden können
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
