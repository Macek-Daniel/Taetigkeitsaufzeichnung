using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Taetigkeitsaufzeichnung.Data;
using Taetigkeitsaufzeichnung.Models;
using Taetigkeitsaufzeichnung.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

namespace Taetigkeitsaufzeichnung.Controllers
{
    public class TaetigkeitController : Controller
    {
        private readonly TaetigkeitsaufzeichnungContext _context;

        public TaetigkeitController(TaetigkeitsaufzeichnungContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Ruft die Microsoft Azure AD Object ID (OID) des aktuellen Benutzers ab.
        /// </summary>
        private string GetUserObjectId()
        {
            return User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;
        }

        /// <summary>
        /// Ruft Name, E-Mail und andere Daten des aktuellen Benutzers ab.
        /// </summary>
        private (string givenName, string surname, string email) GetUserInfo()
        {
            // Versuchen Sie separate given_name und family_name Claims zu finden
            var givenName = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname")?.Value
                         ?? User.FindFirst("given_name")?.Value
                         ?? "";

            var surname = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname")?.Value
                       ?? User.FindFirst("family_name")?.Value
                       ?? "";

            // Falls nur ein "name" Claim verfügbar ist, splitten Sie ihn
            if (string.IsNullOrEmpty(givenName) && string.IsNullOrEmpty(surname))
            {
                var fullName = User.FindFirst("name")?.Value
                            ?? User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value
                            ?? "User";

                var parts = fullName.Split(' ', 2);
                givenName = parts[0];
                surname = parts.Length > 1 ? parts[1] : "";
            }

            // Falls noch immer nur givenName, aber kein surname, setzen Sie defaults
            if (string.IsNullOrEmpty(givenName))
                givenName = "User";

            // Versuchen Sie verschiedene E-Mail Claims
            var email = User.FindFirst(ClaimTypes.Email)?.Value
                     ?? User.FindFirst("email")?.Value
                     ?? User.FindFirst("preferred_username")?.Value
                     ?? User.FindFirst("upn")?.Value
                     ?? "";

            return (givenName, surname, email);
        }

        /// <summary>
        /// Registriert einen neuen Lehrer automatisch, wenn dieser sich zum ersten Mal anmeldet.
        /// </summary>
        private async Task<Lehrer> EnsureUserExists()
        {
            if (!User.Identity?.IsAuthenticated == true)
                return null;

            var objectId = GetUserObjectId();
            if (string.IsNullOrEmpty(objectId))
                return null;

            try
            {
                // Prüfen, ob Lehrer bereits existiert
                var existingLehrer = await _context.Lehrer
                    .FirstOrDefaultAsync(l => l.LehrerID == objectId);

                if (existingLehrer != null)
                    return existingLehrer;

                // Neuen Lehrer erstellen
                var (givenName, surname, email) = GetUserInfo();
                
                var newLehrer = new Lehrer
                {
                    LehrerID = objectId,
                    Vorname = givenName,
                    Nachname = surname,
                    Email = email,
                    LoginName = email,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                _context.Add(newLehrer);
                await _context.SaveChangesAsync();

                return newLehrer;
            }
            catch (Exception ex)
            {
                // Falls Datenbankmigrationen noch nicht durchgeführt wurden, log den Fehler
                System.Diagnostics.Debug.WriteLine($"Error in EnsureUserExists: {ex.Message}");
                return null;
            }
        }

        private async Task PrepareDashboardData(TaetigkeitDashboardViewModel vm)
        {
            // Stelle sicher, dass der Benutzer in der Datenbank existiert
            var currentLehrer = await EnsureUserExists();

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
                    Value = l.LehrerID,
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

            // Setze den aktuellen Lehrer
            if (currentLehrer != null)
            {
                vm.CurrentLehrerID = currentLehrer.LehrerID;
                vm.CurrentLehrerName = $"{currentLehrer.Vorname} {currentLehrer.Nachname}";
                vm.NeueTaetigkeit.LehrerID = currentLehrer.LehrerID;
            }
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

        // POST: Taetigkeit/CreateProjekt
        [HttpPost]
        public async Task<IActionResult> CreateProjekt([FromBody] CreateProjektRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Projektname))
                return BadRequest(new { message = "Projektname erforderlich" });

            try
            {
                var projekt = new Projekt { Projektname = request.Projektname };
                _context.Add(projekt);
                await _context.SaveChangesAsync();

                return Json(new { projektID = projekt.ProjektID, projektname = projekt.Projektname });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    public class CreateProjektRequest
    {
        public string Projektname { get; set; }
    }
}