using Microsoft.EntityFrameworkCore;
using Taetigkeitsaufzeichnung.Models; 

namespace Taetigkeitsaufzeichnung.Data
{
    public class TaetigkeitsaufzeichnungContext : DbContext
    {
        public TaetigkeitsaufzeichnungContext(DbContextOptions<TaetigkeitsaufzeichnungContext> options)
            : base(options)
        {
        }

        public DbSet<Lehrer> Lehrer { get; set; }
        public DbSet<Schuljahr> Schuljahre { get; set; }
        public DbSet<Projekt> Projekte { get; set; }
        public DbSet<Taetigkeit> Taetigkeiten { get; set; }
        public DbSet<LehrerSchuljahrSollstunden> LehrerSchuljahrSollstunden { get; set; }
        public DbSet<Abteilung> Abteilungen { get; set; }
        public DbSet<Abteilungsvorstand> Abteilungsvorstaende { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Align table names with the SQL script (singular names where applicable)
            modelBuilder.Entity<Lehrer>().ToTable("Lehrer");
            modelBuilder.Entity<Schuljahr>().ToTable("Schuljahr");
            modelBuilder.Entity<Projekt>().ToTable("Projekt");
            modelBuilder.Entity<Taetigkeit>().ToTable("Taetigkeit");
            modelBuilder.Entity<LehrerSchuljahrSollstunden>().ToTable("LehrerSchuljahrSollstunden");
            modelBuilder.Entity<Abteilung>().ToTable("Abteilung");
            modelBuilder.Entity<Abteilungsvorstand>().ToTable("Abteilungsvorstand");

            modelBuilder.Entity<LehrerSchuljahrSollstunden>()
                .HasKey(lss => new { lss.LehrerID, lss.SchuljahrID });

            modelBuilder.Entity<Taetigkeit>()
                .HasOne(t => t.Lehrer)
                .WithMany(l => l.Taetigkeiten)
                .HasForeignKey(t => t.LehrerID)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<Taetigkeit>()
                .HasOne(t => t.Projekt)
                .WithMany(p => p.Taetigkeiten)
                .HasForeignKey(t => t.ProjektID)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<LehrerSchuljahrSollstunden>()
                .HasOne(lss => lss.Lehrer)
                .WithMany(l => l.SchuljahrSollstunden)
                .HasForeignKey(lss => lss.LehrerID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LehrerSchuljahrSollstunden>()
                .HasOne(lss => lss.Schuljahr)
                .WithMany(s => s.LehrerSollstunden)
                .HasForeignKey(lss => lss.SchuljahrID)
                .OnDelete(DeleteBehavior.Cascade);

            // Abteilungsvorstand-Beziehungen
            modelBuilder.Entity<Abteilungsvorstand>()
                .HasOne(av => av.Lehrer)
                .WithMany(l => l.Abteilungsvorstaende)
                .HasForeignKey(av => av.LehrerID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Abteilungsvorstand>()
                .HasOne(av => av.Abteilung)
                .WithMany(a => a.Vorstaende)
                .HasForeignKey(av => av.AbteilungID)
                .OnDelete(DeleteBehavior.Restrict);

            // Dezimaltypen explizit konfigurieren
            modelBuilder.Entity<LehrerSchuljahrSollstunden>()
                .Property(lss => lss.Sollstunden)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Taetigkeit>()
                .Property(t => t.DauerStunden)
                .HasPrecision(18, 2);
        }
    }
}