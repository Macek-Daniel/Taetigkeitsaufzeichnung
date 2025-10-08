using Microsoft.AspNetCore.Mvc;

namespace Taetigkeitsaufzeichnung.Controllers;

public class LehrerverwaltungController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}