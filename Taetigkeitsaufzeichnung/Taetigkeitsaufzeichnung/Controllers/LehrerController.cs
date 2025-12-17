using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Taetigkeitsaufzeichnung.Data;
using Taetigkeitsaufzeichnung.Models;
using Taetigkeitsaufzeichnung.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace Taetigkeitsaufzeichnung.Controllers
{
    public class LehrerController : Controller
    {
        private readonly TaetigkeitsaufzeichnungContext _context;

        public LehrerController(TaetigkeitsaufzeichnungContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            int currentSchuljahrId = 2;
            var lehrer = await _context.Lehrer
                .OrderBy(l => l.Nachname)
                .ThenBy(l => l.Vorname)
                .Select(l => new LehrerIndexViewModel
                {
                    LehrerID = l.LehrerID,
                    Vorname = l.Vorname,
                    Nachname = l.Nachname,
                    IsActive = l.IsActive,
                    Sollstunden = _context.LehrerSchuljahrSollstunden
                        .Where(ls => ls.LehrerID == l.LehrerID && ls.SchuljahrID == currentSchuljahrId)
                        .Select(ls => ls.Sollstunden)
                        .FirstOrDefault()                })
                .ToListAsync();
            
            return View(lehrer); 
        }

        public IActionResult Create()
        {
            return View(new LehrerCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LehrerCreateViewModel vm)
        {
            int currentSchuljahrId = 2;
            if (ModelState.IsValid)
            {
                var lehrer = new Lehrer
                {
                    Vorname = vm.Vorname,
                    Nachname = vm.Nachname,
                    IsActive = true 
                };
                
                _context.Add(lehrer);
                _context.SaveChanges();

                var lehrerSchuljahrSollstunden = new LehrerSchuljahrSollstunden
                {
                    Lehrer = lehrer,
                    SchuljahrID = currentSchuljahrId,
                    Sollstunden = vm.Sollstunden
                };
                if (await _context.LehrerSchuljahrSollstunden.FirstOrDefaultAsync(ls => ls.LehrerID == lehrer.LehrerID && ls.SchuljahrID == currentSchuljahrId) == null)
                {
                    _context.Add(lehrerSchuljahrSollstunden);
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(List));
            }
            return View(vm); 
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lehrer = await _context.Lehrer.FindAsync(id);
            if (lehrer == null)
            {
                return NotFound();
            }

            var vm = new LehrerEditViewModel
            {
                LehrerID = lehrer.LehrerID,
                Vorname = lehrer.Vorname,
                Nachname = lehrer.Nachname,
                IsActive = lehrer.IsActive
            };
            
            return View(vm);
        }

        // POST: Lehrer/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LehrerEditViewModel vm)
        {
            if (id != vm.LehrerID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Lehrer aus DB laden
                    var lehrer = await _context.Lehrer.FindAsync(id);
                    if (lehrer == null)
                    {
                        return NotFound();
                    }

                    // Werte von VM auf Model übertragen
                    lehrer.Vorname = vm.Vorname;
                    lehrer.Nachname = vm.Nachname;
                    lehrer.IsActive = vm.IsActive;

                    _context.Update(lehrer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Lehrer.Any(e => e.LehrerID == vm.LehrerID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(List));
            }
            return View(vm);
        }

        // Anstatt zu löschen, deaktivieren wir Lehrer (wegen IsActive-Property)
        // POST: Lehrer/ToggleActivity/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActivity(int id)
        {
            var lehrer = await _context.Lehrer.FindAsync(id);
            if (lehrer == null)
            {
                return NotFound();
            }

            lehrer.IsActive = !lehrer.IsActive; // Status umkehren
            _context.Update(lehrer);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(List));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var lehrer = await _context.Lehrer
                .Include(l => l.Taetigkeiten)
                .Include(l => l.SchuljahrSollstunden)
                .FirstOrDefaultAsync(l => l.LehrerID == id);
                    
            if (lehrer == null)
            {
                return NotFound();
            }

            // Remove all related records first
            _context.Taetigkeiten.RemoveRange(lehrer.Taetigkeiten);
            _context.LehrerSchuljahrSollstunden.RemoveRange(lehrer.SchuljahrSollstunden);
                
            // Then remove the teacher
            _context.Lehrer.Remove(lehrer);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(List));
        }
        
        public async Task<IActionResult> List(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;

            var lehrerQuery = _context.Lehrer.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                lehrerQuery = lehrerQuery.Where(s => s.Nachname.Contains(searchString) 
                                                     || s.Vorname.Contains(searchString));
            }

            var lehrerEntities = await lehrerQuery.ToListAsync();

            // Mapping
            var lehrerListe = lehrerEntities.Select(l => new LehrerIndexViewModel
            {
                LehrerID = l.LehrerID,
                Vorname = l.Vorname,
                Nachname = l.Nachname,
                Sollstunden = 23
                //IstStunden = 0 
            }).ToList();

            var model = new LehrerDashboardViewModel
            {
                LehrerListe = lehrerListe,
                GesamtFehlend = lehrerListe.Sum(x => x.FehlendeStunden),
                AnzahlLehrerErreicht = lehrerListe.Count(x => x.IstErreicht),
                AnzahlLehrerGesamt = lehrerListe.Count
            };

            return View(model);
        }
    }
}