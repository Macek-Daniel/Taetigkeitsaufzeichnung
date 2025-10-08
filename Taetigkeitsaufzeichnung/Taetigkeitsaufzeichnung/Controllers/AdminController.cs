using Microsoft.AspNetCore.Mvc;

namespace Taetigkeitsaufzeichnung.Controllers;

public class AdminController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}