using Microsoft.EntityFrameworkCore;
using TechVerseX.TechnologyService.Domain;

namespace TechVerseX.TechnologyService.Infrastructure.Persistence;

/// <summary>
/// Basis data bounded context Technology.
/// </summary>
/// <remarks>
/// Memakai <b>schema per bounded context</b> di dalam SATU PostgreSQL — persis
/// tahap awal yang digambarkan KERANGKA.md 4.7 (Database Ownership). Pemisahan
/// jadi basis data terpisah baru dilakukan kalau skalanya menuntut, dan
/// batas schema ini yang membuat pemisahan itu nanti murah.
/// </remarks>
public sealed class TechnologyDbContext(DbContextOptions<TechnologyDbContext> options) : DbContext(options)
{
    public const string Schema = "technology";

    public DbSet<Technology> Technologies => Set<Technology>();

    public DbSet<TechnologyRelationship> Relationships => Set<TechnologyRelationship>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TechnologyDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
