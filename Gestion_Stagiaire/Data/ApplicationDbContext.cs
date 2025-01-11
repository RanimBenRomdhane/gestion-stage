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
        public DbSet<Affectation> Affectations { get; set; }
        public DbSet<Departement> Departements { get; set; }
        public DbSet<Status> Statuses { get; set; }
        public DbSet<Type_Stage> TypesStage { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Stagiaire entity
            modelBuilder.Entity<Stagiaire>()
                .HasKey(s => s.Id);

            modelBuilder.Entity<Stagiaire>()
                .HasMany(s => s.DemandesStage)
                .WithOne(d => d.Stagiaire)
                .HasForeignKey(d => d.StagiaireId);

            // Configure DemandeStage entity
            modelBuilder.Entity<DemandeStage>()
                .HasKey(d => d.Id);

            modelBuilder.Entity<DemandeStage>()
             .HasOne(d => d.Affectation)
             .WithOne(a => a.DemandeStage)
             .HasForeignKey<Affectation>(a => a.DemandeStageId)
             .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DemandeStage>()
                .HasOne(d => d.Status)
                .WithMany(s => s.DemandesStage)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.Restrict);

           modelBuilder.Entity<DemandeStage>()
        .HasOne(d => d.Type_Stage)
        .WithMany(t => t.DemandesStage)
        .HasForeignKey(d => d.Type_StageId);

            // Configure Affectation entity
            modelBuilder.Entity<Affectation>()
                .HasKey(a => a.Id);

            modelBuilder.Entity<Affectation>()
                .HasMany(a => a.Departement)
                .WithMany(d => d.Affectations);

            // Configure Departement entity
            modelBuilder.Entity<Departement>()
                .HasKey(d => d.Id);

            // Configure Status entity
            modelBuilder.Entity<Status>()
                .HasKey(s => s.Id);

            // Configure Type_Stage entity
            modelBuilder.Entity<Type_Stage>()
                .HasKey(t => t.Id);
        }
    }
}

