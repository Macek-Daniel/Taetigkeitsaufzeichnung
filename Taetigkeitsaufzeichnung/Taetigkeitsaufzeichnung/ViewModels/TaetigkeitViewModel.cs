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

        [Required(ErrorMessage = "Bitte geben Sie eine Beschreibung ein.")]
        [StringLength(500, ErrorMessage = "Beschreibung darf nicht länger als 500 Zeichen sein.")]
        public string Beschreibung { get; set; }

        [Required(ErrorMessage = "Bitte geben Sie die Dauer an.")]
        [Range(0.25, 24, ErrorMessage = "Dauer muss zwischen 0,25 und 24 Stunden liegen.")]
        [Display(Name = "Dauer (Stunden)")]
        public decimal DauerStunden { get; set; }

        [Required(ErrorMessage = "Bitte wählen Sie einen Lehrer aus.")]
        [Display(Name = "Lehrer")]
        public string LehrerID { get; set; }

        [Required(ErrorMessage = "Bitte wählen Sie ein Projekt aus.")]
        [Display(Name = "Projekt")]
        public int ProjektID { get; set; }
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

    public class TaetigkeitDashboardViewModel
    {
        public TaetigkeitCreateEditViewModel NeueTaetigkeit { get; set; } = new TaetigkeitCreateEditViewModel();
        public IEnumerable<TaetigkeitIndexViewModel> Historie { get; set; } = new List<TaetigkeitIndexViewModel>();
        
        public IEnumerable<SelectListItem> LehrerListe { get; set; }
        public IEnumerable<SelectListItem> ProjektListe { get; set; }

        public string CurrentLehrerID { get; set; }
        public string CurrentLehrerName { get; set; }
    }
}