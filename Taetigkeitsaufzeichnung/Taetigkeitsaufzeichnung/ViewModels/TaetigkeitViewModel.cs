using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Taetigkeitsaufzeichnung.Models; 

namespace Taetigkeitsaufzeichnung.ViewModels
{
    public class TaetigkeitCreateEditViewModel
    {
        public int TaetigkeitID { get; set; }

        [Required]
        public DateOnly Datum { get; set; } = DateOnly.FromDateTime(DateTime.Now);

        [Required]
        [StringLength(500, ErrorMessage = "Beschreibung darf nicht länger als 500 Zeichen sein.")]
        public string Beschreibung { get; set; }

        [Required]
        [Range(0.25, 24, ErrorMessage = "Dauer muss zwischen 0,25 und 24 Stunden liegen.")]
        [Display(Name = "Dauer (in Stunden)")]
        public decimal DauerStunden { get; set; }

        [Required(ErrorMessage = "Bitte wählen Sie einen Lehrer aus.")]
        [Display(Name = "Lehrer")]
        public int LehrerID { get; set; }

        [Required(ErrorMessage = "Bitte wählen Sie ein Projekt aus.")]
        [Display(Name = "Projekt")]
        public int ProjektID { get; set; }

        public IEnumerable<SelectListItem> LehrerListe { get; set; }
        public IEnumerable<SelectListItem> ProjektListe { get; set; }
    }

    public class TaetigkeitIndexViewModel
    {
        public int TaetigkeitID { get; set; }
        public DateOnly Datum { get; set; }
        public string Beschreibung { get; set; }
        public decimal DauerStunden { get; set; }
        public string LehrerName { get; set; }
        public string ProjektName { get; set; }
    }
}