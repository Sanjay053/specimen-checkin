using Microsoft.EntityFrameworkCore;
using SpecimenCheckIn.Api.Models;

namespace SpecimenCheckIn.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Lab> Labs => Set<Lab>();

    public DbSet<Manifest> Manifests => Set<Manifest>();

    public DbSet<Specimen> Specimens => Set<Specimen>();

    public DbSet<Discrepancy> Discrepancies => Set<Discrepancy>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Manifest>()
            .HasOne(m => m.Lab)
            .WithMany(l => l.Manifests)
            .HasForeignKey(m => m.LabId);

        modelBuilder.Entity<Specimen>()
            .HasOne(s => s.Manifest)
            .WithMany(m => m.Specimens)
            .HasForeignKey(s => s.ManifestId);

        modelBuilder.Entity<Discrepancy>()
            .HasOne(d => d.Manifest)
            .WithMany(m => m.Discrepancies)
            .HasForeignKey(d => d.ManifestId);

        modelBuilder.Entity<Discrepancy>()
            .HasOne(d => d.Specimen)
            .WithMany()
            .HasForeignKey(d => d.SpecimenId);
    }
}