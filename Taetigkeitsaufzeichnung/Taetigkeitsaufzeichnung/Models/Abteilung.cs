using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Taetigkeitsaufzeichnung.Models
{
    public class Abteilung
    {
        public int AbteilungID { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(20)]
        public string Kuerzel { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<Abteilungsvorstand> Vorstaende { get; set; } = new List<Abteilungsvorstand>();
    }
}
