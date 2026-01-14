using System.ComponentModel.DataAnnotations;

namespace Taetigkeitsaufzeichnung.ViewModels
{
    public class LehrerIndexViewModel
    {
        public string LehrerID { get; set; }
        public string Vorname { get; set; }
        public string Nachname { get; set; }
        public bool IsActive { get; set; }
        public string FullName => $"{Vorname} {Nachname}";
        public decimal Sollstunden { get; set; }
        public decimal IstStunden { get; set; }
        public decimal FehlendeStunden => Math.Max(Sollstunden - IstStunden, 0);

        // 3. Prüft automatisch, ob das Ziel erreicht wurde (true/false)
        public bool IstErreicht => IstStunden >= Sollstunden;
        public int AnzahlAbteilungsvorstaende { get; set; }
    }

    public class LehrerCreateViewModel
    {
        [Required(ErrorMessage = "Vorname ist erforderlich")]
        [StringLength(100)]
        public string Vorname { get; set; }

        [Required(ErrorMessage = "Nachname ist erforderlich")]
        [StringLength(100)]
        public string Nachname { get; set; }
        public int Sollstunden { get; set; }
    }

    public class LehrerEditViewModel
    {
        public string LehrerID { get; set; }

        [Required(ErrorMessage = "Vorname ist erforderlich")]
        [StringLength(100)]
        public string Vorname { get; set; }

        [Required(ErrorMessage = "Nachname ist erforderlich")]
        [StringLength(100)]
        public string Nachname { get; set; }

        [Display(Name = "Aktiv")]
        public bool IsActive { get; set; }

        [Display(Name = "Sollstunden")]
        [Range(0, 999, ErrorMessage = "Sollstunden muss zwischen 0 und 999 liegen")]
        public decimal Sollstunden { get; set; }
    }

    public class LehrerDashboardViewModel
    {
        public List<LehrerIndexViewModel> LehrerListe { get; set; } = new();
        public decimal GesamtFehlend { get; set; }
        public int AnzahlLehrerErreicht { get; set; }
        public int AnzahlLehrerGesamt { get; set; }
    }
}