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
        public DbSet<LehrerSchuljahrSollstunde> LehrerSchuljahrSollstunden { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LehrerSchuljahrSollstunde>()
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
            
            modelBuilder.Entity<LehrerSchuljahrSollstunde>()
                .HasOne(lss => lss.Lehrer)
                .WithMany(l => l.SchuljahrSollstunden)
                .HasForeignKey(lss => lss.LehrerID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LehrerSchuljahrSollstunde>()
                .HasOne(lss => lss.Schuljahr)
                .WithMany(s => s.LehrerSollstunden)
                .HasForeignKey(lss => lss.SchuljahrID)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}