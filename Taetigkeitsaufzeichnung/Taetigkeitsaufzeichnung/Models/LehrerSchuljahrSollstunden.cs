using System.ComponentModel.DataAnnotations.Schema;

namespace Taetigkeitsaufzeichnung.Models
{
    public class LehrerSchuljahrSollstunden
    {
        public int LehrerID { get; set; }
        public int SchuljahrID { get; set; }

        public decimal Sollstunden { get; set; }

        // Navigations-Properties
        public Lehrer Lehrer { get; set; }
        public Schuljahr Schuljahr { get; set; }
    }
}