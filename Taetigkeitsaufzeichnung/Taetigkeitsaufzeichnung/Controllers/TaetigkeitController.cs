using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Taetigkeitsaufzeichnung.Data;
using Taetigkeitsaufzeichnung.Models;
using Taetigkeitsaufzeichnung.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Taetigkeitsaufzeichnung.Controllers
{
    public class TaetigkeitController : Controller
    {
        private readonly TaetigkeitsaufzeichnungContext _context;

        public TaetigkeitController(TaetigkeitsaufzeichnungContext context)
        {
            _context = context;
        }

        private async Task PrepareDashboardData(TaetigkeitDashboardViewModel vm)
        {
            vm.Historie = await _context.Taetigkeiten
                .Include(t => t.Lehrer)
                .Include(t => t.Projekt)
                .OrderByDescending(t => t.Datum)
                .ThenByDescending(t => t.TaetigkeitID)
                .Select(t => new TaetigkeitIndexViewModel
                {
                    TaetigkeitID = t.TaetigkeitID,
                    Datum = t.Datum,
                    Beschreibung = t.Beschreibung,
                    DauerStunden = t.DauerStunden,
                    LehrerName = t.Lehrer.Vorname + " " + t.Lehrer.Nachname,
                    ProjektName = t.Projekt.Projektname
                })
                .ToListAsync();

            vm.LehrerListe = await _context.Lehrer
                .Where(l => l.IsActive)
                .OrderBy(l => l.Nachname)
                .Select(l => new SelectListItem
                {
                    Value = l.LehrerID.ToString(),
                    Text = l.Vorname + " " + l.Nachname
                })
                .ToListAsync();

            vm.ProjektListe = await _context.Projekte
                .OrderBy(p => p.Projektname)
                .Select(p => new SelectListItem
                {
                    Value = p.ProjektID.ToString(),
                    Text = p.Projektname
                })
                .ToListAsync();
        }

        // GET: Taetigkeit/Index (Dashboard)
        public async Task<IActionResult> Index()
        {
            var vm = new TaetigkeitDashboardViewModel();
            await PrepareDashboardData(vm);
            return View(vm);
        }

        // POST: Taetigkeit/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind(Prefix = "NeueTaetigkeit")] TaetigkeitCreateEditViewModel neueTaetigkeit)
        {
            if (ModelState.IsValid)
            {
                var taetigkeit = new Taetigkeit
                {
                    Datum = neueTaetigkeit.Datum,
                    Beschreibung = neueTaetigkeit.Beschreibung,
                    DauerStunden = neueTaetigkeit.DauerStunden,
                    LehrerID = neueTaetigkeit.LehrerID,
                    ProjektID = neueTaetigkeit.ProjektID
                };

                _context.Add(taetigkeit);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var vm = new TaetigkeitDashboardViewModel();
            vm.NeueTaetigkeit = neueTaetigkeit;
            
            await PrepareDashboardData(vm);

            return View("Index", vm);
        }

        // POST: Taetigkeit/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var taetigkeit = await _context.Taetigkeiten.FindAsync(id);
            if (taetigkeit != null)
            {
                _context.Taetigkeiten.Remove(taetigkeit);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}