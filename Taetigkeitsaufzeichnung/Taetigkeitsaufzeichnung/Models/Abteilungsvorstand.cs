using System;
using System.ComponentModel.DataAnnotations;

namespace Taetigkeitsaufzeichnung.Models
{
    public class Abteilungsvorstand
    {
        public int AbteilungsvorstandID { get; set; }

        [Required]
        public int AbteilungID { get; set; }

        [Required]
        public int LehrerID { get; set; }

        public DateOnly? StartDatum { get; set; }
        public DateOnly? EndDatum { get; set; }

        public Abteilung Abteilung { get; set; }
        public Lehrer Lehrer { get; set; }
    }
}
