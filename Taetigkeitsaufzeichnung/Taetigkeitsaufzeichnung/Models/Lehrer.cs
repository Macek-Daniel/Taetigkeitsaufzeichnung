using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Taetigkeitsaufzeichnung.Models
{
    public class Lehrer
    {
        [Key]
        [StringLength(36)]
        public string LehrerID { get; set; }

        [Required]
        [StringLength(100)]
        public string Vorname { get; set; }

        [Required]
        [StringLength(100)]
        public string Nachname { get; set; }

        [Required]
        [StringLength(256)]
        public string Email { get; set; }

        [StringLength(256)]
        public string LoginName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        public ICollection<Taetigkeit> Taetigkeiten { get; set; }
        public ICollection<LehrerSchuljahrSollstunden> SchuljahrSollstunden { get; set; }
        public ICollection<Abteilungsvorstand> Abteilungsvorstaende { get; set; } = new List<Abteilungsvorstand>();
    }
}