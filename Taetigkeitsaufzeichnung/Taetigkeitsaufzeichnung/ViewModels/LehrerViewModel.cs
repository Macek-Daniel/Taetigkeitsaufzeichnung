using System.ComponentModel.DataAnnotations;

namespace Taetigkeitsaufzeichnung.ViewModels
{
    public class LehrerIndexViewModel
    {
        public int LehrerID { get; set; }
        public string Vorname { get; set; }
        public string Nachname { get; set; }
        public bool IsActive { get; set; }
        public string FullName => $"{Vorname} {Nachname}";
        public decimal Sollstunden { get; set; }

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
        public int LehrerID { get; set; }

        [Required(ErrorMessage = "Vorname ist erforderlich")]
        [StringLength(100)]
        public string Vorname { get; set; }

        [Required(ErrorMessage = "Nachname ist erforderlich")]
        [StringLength(100)]
        public string Nachname { get; set; }

        [Display(Name = "Aktiv")]
        public bool IsActive { get; set; }
    }
}