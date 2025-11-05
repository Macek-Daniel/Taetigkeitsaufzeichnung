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

        public async Task<IActionResult> Index()
        {
            var taetigkeiten = await _context.Taetigkeiten
                .Include(t => t.Lehrer)
                .Include(t => t.Projekt) 
                .OrderByDescending(t => t.Datum)
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

            return View(taetigkeiten);
        }

        public async Task<IActionResult> Create()
        {
            var vm = new TaetigkeitCreateEditViewModel();
            await PopulateDropdowns(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaetigkeitCreateEditViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var taetigkeit = new Taetigkeit
                {
                    Datum = vm.Datum,
                    Beschreibung = vm.Beschreibung,
                    DauerStunden = vm.DauerStunden,
                    LehrerID = vm.LehrerID,
                    ProjektID = vm.ProjektID
                };

                _context.Add(taetigkeit);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropdowns(vm);
            return View(vm);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var taetigkeit = await _context.Taetigkeiten.FindAsync(id);
            if (taetigkeit == null) return NotFound();

            var vm = new TaetigkeitCreateEditViewModel
            {
                TaetigkeitID = taetigkeit.TaetigkeitID,
                Datum = taetigkeit.Datum,
                Beschreibung = taetigkeit.Beschreibung,
                DauerStunden = taetigkeit.DauerStunden,
                LehrerID = taetigkeit.LehrerID,
                ProjektID = taetigkeit.ProjektID
            };
            
            await PopulateDropdowns(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TaetigkeitCreateEditViewModel vm)
        {
            if (id != vm.TaetigkeitID) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var taetigkeit = await _context.Taetigkeiten.FindAsync(id);
                    if (taetigkeit == null) return NotFound();

                    taetigkeit.Datum = vm.Datum;
                    taetigkeit.Beschreibung = vm.Beschreibung;
                    taetigkeit.DauerStunden = vm.DauerStunden;
                    taetigkeit.LehrerID = vm.LehrerID;
                    taetigkeit.ProjektID = vm.ProjektID;

                    _context.Update(taetigkeit);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Taetigkeiten.Any(e => e.TaetigkeitID == vm.TaetigkeitID))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            await PopulateDropdowns(vm);
            return View(vm);
        }
        
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var taetigkeit = await _context.Taetigkeiten.FindAsync(id);
            if (taetigkeit != null)
            {
                _context.Taetigkeiten.Remove(taetigkeit);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDropdowns(TaetigkeitCreateEditViewModel vm)
        {
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
    }
}