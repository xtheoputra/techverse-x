namespace TechVerseX.TechnologyService.Domain;

/// <summary>
/// Satu sisi berarah di knowledge graph: <c>(:Technology)-[:REQUIRES]->(:Technology)</c>.
/// </summary>
/// <remarks>
/// 🔑 <b>Arahnya: <see cref="FromTechnologyId"/> membutuhkan
/// <see cref="ToTechnologyId"/></b> — topik tujuan dipelajari lebih dulu. Satu
/// fakta disimpan SEKALI; kebalikannya ("dibutuhkan oleh") tidak pernah ditulis,
/// ia diturunkan saat dibaca dari <see cref="ToTechnologyId"/> (ADR-023). Kebalikan
/// yang ditulis tangan adalah jalan dua baris saling bertentangan.
/// <para>
/// Sisi ini anak agregat topik ASAL, jadi satu-satunya jalan menulisnya adalah
/// <see cref="Technology.RequireTopic"/>. Karena itu <see cref="Create"/>
/// <c>internal</c>, sama seperti <see cref="RoadmapStep"/>, <see cref="TechnologyTool"/>,
/// <see cref="Project"/>, dan <see cref="Resource"/>.
/// </para>
/// <para>
/// Disimpan di PostgreSQL, bukan Neo4j. Itu bukan kelalaian — KERANGKA.md 4.6
/// menetapkan PostgreSQL sebagai <em>source of truth</em> dan Neo4j sebagai
/// <em>derived knowledge</em>. Proyeksi ke Neo4j baru dibangun di EPIC 10.
/// </para>
/// </remarks>
public sealed class TechnologyRelationship
{
    private TechnologyRelationship()
    {
    }

    private TechnologyRelationship(Guid id, Guid fromTechnologyId, Guid toTechnologyId, RelationshipKind kind)
    {
        Id = id;
        FromTechnologyId = fromTechnologyId;
        ToTechnologyId = toTechnologyId;
        Kind = kind;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    /// <summary>Topik yang membutuhkan.</summary>
    public Guid FromTechnologyId { get; private set; }

    /// <summary>Topik yang dibutuhkan — dipelajari lebih dulu.</summary>
    public Guid ToTechnologyId { get; private set; }

    public RelationshipKind Kind { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// Dipanggil <see cref="Technology"/> saja. <c>internal</c> supaya tidak ada
    /// sisi yang lahir di luar agregat — dan melewati aturan dua arahnya.
    /// </summary>
    internal static TechnologyRelationship Create(Guid fromTechnologyId, Guid toTechnologyId, RelationshipKind kind)
    {
        if (fromTechnologyId == toTechnologyId)
        {
            throw new ArgumentException("Sebuah teknologi tidak boleh berhubungan dengan dirinya sendiri.", nameof(toTechnologyId));
        }

        return new TechnologyRelationship(Guid.CreateVersion7(), fromTechnologyId, toTechnologyId, kind);
    }
}
