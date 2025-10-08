using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Taetigkeitsaufzeichnung.Models
{
    [Index(nameof(Projektname), IsUnique = true)]
    public class Projekt
    {
        public int ProjektID { get; set; }

        [Required]
        [StringLength(200)]
        public string Projektname { get; set; }

        public ICollection<Taetigkeit> Taetigkeiten { get; set; }
    }
}