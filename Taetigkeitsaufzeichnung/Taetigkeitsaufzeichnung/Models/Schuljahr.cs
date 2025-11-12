using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Taetigkeitsaufzeichnung.Models
{
    [Index(nameof(Bezeichnung), IsUnique = true)] // UNIQUE Constraint
    public class Schuljahr
    {
        public int SchuljahrID { get; set; }

        [Required]
        [StringLength(20)]
        public string Bezeichnung { get; set; }

        [Required]
        public DateOnly Startdatum { get; set; } 

        [Required]
        public DateOnly Enddatum { get; set; } 

        public ICollection<LehrerSchuljahrSollstunden> LehrerSollstunden { get; set; }
    }
}