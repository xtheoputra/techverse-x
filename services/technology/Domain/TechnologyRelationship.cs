namespace TechVerseX.TechnologyService.Domain;

/// <summary>
/// Satu sisi berarah di knowledge graph: <c>(:Technology)-[:USES]->(:Technology)</c>.
/// </summary>
/// <remarks>
/// Disimpan di PostgreSQL, bukan Neo4j. Itu bukan kelalaian — KERANGKA.md 4.6
/// menetapkan PostgreSQL sebagai <em>source of truth</em> dan Neo4j sebagai
/// <em>derived knowledge</em>. Proyeksi ke Neo4j baru dibangun di EPIC 10.
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

    public Guid FromTechnologyId { get; private set; }

    public Guid ToTechnologyId { get; private set; }

    public RelationshipKind Kind { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public static TechnologyRelationship Create(Guid fromTechnologyId, Guid toTechnologyId, RelationshipKind kind)
    {
        if (fromTechnologyId == toTechnologyId)
        {
            throw new ArgumentException("Sebuah teknologi tidak boleh berhubungan dengan dirinya sendiri.", nameof(toTechnologyId));
        }

        return new TechnologyRelationship(Guid.CreateVersion7(), fromTechnologyId, toTechnologyId, kind);
    }
}
