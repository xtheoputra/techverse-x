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

    public DbSet<Field> Fields => Set<Field>();

    public DbSet<Technology> Technologies => Set<Technology>();

    /// <summary>
    /// Katalog alat. <b>DbSet sendiri, bukan sekadar navigasi dari
    /// <see cref="Technologies"/></b> — satu alat dipakai lintas topik, jadi ia
    /// harus bisa dicari dan disunting tanpa melewati topik mana pun (ADR-015).
    /// </summary>
    /// <remarks>
    /// Ketiga bagian isi yang lain — <see cref="Domain.RoadmapStep"/>,
    /// <see cref="Domain.Project"/>, <see cref="Domain.Resource"/> — sengaja
    /// TIDAK punya <c>DbSet</c>. Mereka hanya berarti di dalam topiknya, dan
    /// memberi mereka pintu masuk sendiri mengundang penulisan yang melewati
    /// agregatnya — persis jalan yang membuat nomor langkah roadmap bisa berlubang.
    /// <para>
    /// Aturan yang sama berlaku untuk <see cref="Domain.TechnologyRelationship"/>
    /// (ADR-023). Ia dulu PUNYA <c>DbSet Relationships</c> publik dengan nol pemanggil
    /// — pintu tulis kedua, tempat sisi bisa ditambahkan tanpa melewati aturan dua
    /// topik tidak boleh saling mensyaratkan. Pintu itu dibuang: sisi ditulis hanya lewat
    /// <see cref="Domain.Technology.RequireTopic"/>, dan dibaca lewat
    /// <c>db.Technologies.SelectMany(t =&gt; t.Relationships)</c>.
    /// </para>
    /// </remarks>
    public DbSet<Tool> Tools => Set<Tool>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TechnologyDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
