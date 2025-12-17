using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Taetigkeitsaufzeichnung.Data;
using Taetigkeitsaufzeichnung.Models;

namespace Taetigkeitsaufzeichnung.Controllers
{
    public class AbteilungController : Controller
    {
        private readonly TaetigkeitsaufzeichnungContext _context;

        public AbteilungController(TaetigkeitsaufzeichnungContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var list = await _context.Abteilungen
                .OrderBy(a => a.Name)
                .ToListAsync();
            return View(list);
        }

        public IActionResult Create()
        {
            return View(new Abteilung());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Abteilung model)
        {
            if (ModelState.IsValid)
            {
                _context.Abteilungen.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var entity = await _context.Abteilungen.FindAsync(id);
            if (entity == null) return NotFound();
            return View(entity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Abteilung model)
        {
            if (id != model.AbteilungID) return NotFound();
            if (!ModelState.IsValid) return View(model);

            var entity = await _context.Abteilungen.FindAsync(id);
            if (entity == null) return NotFound();

            entity.Name = model.Name;
            entity.Kuerzel = model.Kuerzel;
            entity.IsActive = model.IsActive;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.Abteilungen
                .Include(a => a.Vorstaende)
                .FirstOrDefaultAsync(a => a.AbteilungID == id);
            if (entity == null) return NotFound();

            if (entity.Vorstaende.Any())
            {
                TempData["Error"] = "Abteilung kann nicht gelöscht werden, da Abteilungsvorstände vorhanden sind.";
                return RedirectToAction(nameof(Index));
            }

            _context.Abteilungen.Remove(entity);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
