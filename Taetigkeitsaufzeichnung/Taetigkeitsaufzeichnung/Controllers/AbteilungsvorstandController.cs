using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Taetigkeitsaufzeichnung.Data;
using Taetigkeitsaufzeichnung.Models;

namespace Taetigkeitsaufzeichnung.Controllers
{
    public class AbteilungsvorstandController : Controller
    {
        private readonly TaetigkeitsaufzeichnungContext _context;

        public AbteilungsvorstandController(TaetigkeitsaufzeichnungContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var list = await _context.Abteilungsvorstaende
                .Include(av => av.Abteilung)
                .Include(av => av.Lehrer)
                .OrderBy(av => av.Abteilung.Name)
                .ThenBy(av => av.Lehrer.Nachname)
                .ToListAsync();
            return View(list);
        }

        public async Task<IActionResult> Create()
        {
            await FillSelectLists();
            return View(new Abteilungsvorstand());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Abteilungsvorstand model)
        {
            if (ModelState.IsValid)
            {
                _context.Abteilungsvorstaende.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            await FillSelectLists();
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var entity = await _context.Abteilungsvorstaende.FindAsync(id);
            if (entity == null) return NotFound();
            await FillSelectLists();
            return View(entity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Abteilungsvorstand model)
        {
            if (id != model.AbteilungsvorstandID) return NotFound();
            if (!ModelState.IsValid)
            {
                await FillSelectLists();
                return View(model);
            }

            var entity = await _context.Abteilungsvorstaende.FindAsync(id);
            if (entity == null) return NotFound();

            entity.AbteilungID = model.AbteilungID;
            entity.LehrerID = model.LehrerID;
            entity.StartDatum = model.StartDatum;
            entity.EndDatum = model.EndDatum;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.Abteilungsvorstaende.FindAsync(id);
            if (entity == null) return NotFound();
            _context.Abteilungsvorstaende.Remove(entity);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private async Task FillSelectLists()
        {
            var abteilungen = await _context.Abteilungen
                .OrderBy(a => a.Name)
                .Select(a => new { a.AbteilungID, a.Name })
                .ToListAsync();
            var lehrer = await _context.Lehrer
                .OrderBy(l => l.Nachname).ThenBy(l => l.Vorname)
                .Select(l => new { l.LehrerID, Name = l.Vorname + " " + l.Nachname })
                .ToListAsync();

            ViewBag.Abteilungen = new SelectList(abteilungen, "AbteilungID", "Name");
            ViewBag.Lehrer = new SelectList(lehrer, "LehrerID", "Name");
        }
    }
}
