using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Taetigkeitsaufzeichnung.Models
{
    public class Taetigkeit
    {
        public int TaetigkeitID { get; set; }

        [Required]
        public DateOnly Datum { get; set; } 

        [Required]
        public string Beschreibung { get; set; }

        public decimal DauerStunden { get; set; }

        // Foreign Keys
        public string LehrerID { get; set; }
        public int ProjektID { get; set; }

        // Navigations-Properties
        public Lehrer Lehrer { get; set; }
        public Projekt Projekt { get; set; }
    }
}