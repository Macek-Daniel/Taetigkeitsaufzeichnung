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

        private async Task<int> GetActiveSchuljahrIdAsync()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            var current = await _context.Schuljahre
                .Where(s => s.Startdatum <= today && s.Enddatum >= today)
                .OrderByDescending(s => s.SchuljahrID)
                .Select(s => s.SchuljahrID)
                .FirstOrDefaultAsync();

            if (current != 0)
            {
                return current;
            }

            var latest = await _context.Schuljahre
                .OrderByDescending(s => s.SchuljahrID)
                .Select(s => s.SchuljahrID)
                .FirstOrDefaultAsync();

            return latest != 0 ? latest : 2; // Fallback to seeded ID 2 if DB empty
        }

        public async Task<IActionResult> Index()
        {
            int currentSchuljahrId = await GetActiveSchuljahrIdAsync();
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
                        .FirstOrDefault(),
                    IstStunden = _context.Taetigkeiten
                        .Where(t => t.LehrerID == l.LehrerID)
                        .Sum(t => t.DauerStunden),
                    AnzahlAbteilungsvorstaende = _context.Abteilungsvorstaende.Count(av => av.LehrerID == l.LehrerID)
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
            int currentSchuljahrId = await GetActiveSchuljahrIdAsync();
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

            int currentSchuljahrId = await GetActiveSchuljahrIdAsync();
            var sollstunden = await _context.LehrerSchuljahrSollstunden
                .Where(ls => ls.LehrerID == lehrer.LehrerID && ls.SchuljahrID == currentSchuljahrId)
                .Select(ls => ls.Sollstunden)
                .FirstOrDefaultAsync();

            var vm = new LehrerEditViewModel
            {
                LehrerID = lehrer.LehrerID,
                Vorname = lehrer.Vorname,
                Nachname = lehrer.Nachname,
                IsActive = lehrer.IsActive,
                Sollstunden = sollstunden
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

                    // Update Sollstunden for current Schuljahr
                    int currentSchuljahrId = await GetActiveSchuljahrIdAsync();
                    var sollstundenEntry = await _context.LehrerSchuljahrSollstunden
                        .FirstOrDefaultAsync(ls => ls.LehrerID == id && ls.SchuljahrID == currentSchuljahrId);

                    if (sollstundenEntry != null)
                    {
                        sollstundenEntry.Sollstunden = vm.Sollstunden;
                        _context.Update(sollstundenEntry);
                    }
                    else
                    {
                        _context.Add(new LehrerSchuljahrSollstunden
                        {
                            LehrerID = id,
                            SchuljahrID = currentSchuljahrId,
                            Sollstunden = vm.Sollstunden
                        });
                    }

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

        // Hard delete is intentionally disabled; use ToggleActivity instead.

        public async Task<IActionResult> List(string searchString, string statusFilter = "active")
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["StatusFilter"] = statusFilter;

            var lehrerQuery = _context.Lehrer.AsQueryable();

            // Filter by status
            if (statusFilter == "active")
            {
                lehrerQuery = lehrerQuery.Where(l => l.IsActive);
            }
            else if (statusFilter == "inactive")
            {
                lehrerQuery = lehrerQuery.Where(l => !l.IsActive);
            }
            // "all" shows all teachers, no filter needed

            if (!string.IsNullOrEmpty(searchString))
            {
                lehrerQuery = lehrerQuery.Where(s => s.Nachname.Contains(searchString)
                                                     || s.Vorname.Contains(searchString));
            }

            var lehrerEntities = await lehrerQuery.ToListAsync();

            // Use current Schuljahr for Sollstunden lookup
            int currentSchuljahrId = await GetActiveSchuljahrIdAsync();
            var sollstundenMap = await _context.LehrerSchuljahrSollstunden
                .Where(ls => ls.SchuljahrID == currentSchuljahrId)
                .ToDictionaryAsync(ls => ls.LehrerID, ls => ls.Sollstunden);

            // Get IstStunden for all Lehrer
            var istStundenMap = await _context.Taetigkeiten
                .GroupBy(t => t.LehrerID)
                .Select(g => new { LehrerID = g.Key, IstStunden = g.Sum(t => t.DauerStunden) })
                .ToDictionaryAsync(x => x.LehrerID, x => x.IstStunden);

            // Mapping
            var lehrerListe = lehrerEntities.Select(l => new LehrerIndexViewModel
            {
                LehrerID = l.LehrerID,
                Vorname = l.Vorname,
                Nachname = l.Nachname,
                IsActive = l.IsActive,
                Sollstunden = sollstundenMap.TryGetValue(l.LehrerID, out var s) ? s : 0,
                IstStunden = istStundenMap.TryGetValue(l.LehrerID, out var ist) ? ist : 0,
                AnzahlAbteilungsvorstaende = _context.Abteilungsvorstaende.Count(av => av.LehrerID == l.LehrerID)
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