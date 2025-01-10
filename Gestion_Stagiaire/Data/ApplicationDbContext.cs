using Gestion_Stagiaire.Models;
using Gestion_Stagiaires.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Gestion_Stagiaire.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Stagiaire> Stagiaires { get; set; }
        public DbSet<DemandeStage> DemandesStage { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships and table mappings
            modelBuilder.Entity<Stagiaire>()
                .HasMany(s => s.DemandesStage)
                .WithOne(d => d.Stagiaire)
                .HasForeignKey(d => d.StagiaireId);

            modelBuilder.Entity<DemandeStage>()
                .HasKey(d => d.Id);

            modelBuilder.Entity<Stagiaire>()
                .HasKey(s => s.Id);

            // Additional configurations (if needed)
        }
    
    }
}
