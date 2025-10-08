using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Taetigkeitsaufzeichnung.Models
{
    public class Lehrer
    {
        public int LehrerID { get; set; }

        [Required]
        [StringLength(100)]
        public string Vorname { get; set; }

        [Required]
        [StringLength(100)]
        public string Nachname { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<Taetigkeit> Taetigkeiten { get; set; }
        public ICollection<LehrerSchuljahrSollstunde> SchuljahrSollstunden { get; set; }
    }
}