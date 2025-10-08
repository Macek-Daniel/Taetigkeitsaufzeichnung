using Microsoft.AspNetCore.Mvc;

namespace Taetigkeitsaufzeichnung.Controllers;

public class LehrerController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}