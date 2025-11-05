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
            var lehrer = await _context.Lehrer
                .OrderBy(l => l.Nachname)
                .ThenBy(l => l.Vorname)
                .Select(l => new LehrerIndexViewModel
                {
                    LehrerID = l.LehrerID,
                    Vorname = l.Vorname,
                    Nachname = l.Nachname,
                    IsActive = l.IsActive
                })
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
            if (ModelState.IsValid)
            {
                var lehrer = new Lehrer
                {
                    Vorname = vm.Vorname,
                    Nachname = vm.Nachname,
                    IsActive = true 
                };

                _context.Add(lehrer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
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
                return RedirectToAction(nameof(Index));
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

            return RedirectToAction(nameof(Index));
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

            return RedirectToAction(nameof(Index));
        }
    }
}