namespace TechVerseX.TechnologyService.Domain;

/// <summary>
/// Jenis hubungan antar-teknologi.
/// Diambil apa adanya dari graph model di KERANGKA.md 4.6 (Knowledge Graph):
/// <c>requires</c>, <c>uses</c>, <c>related</c>, <c>used_by</c>, <c>enables</c>.
/// </summary>
public enum RelationshipKind
{
    Requires = 0,
    Uses = 1,
    RelatedTo = 2,
    UsedBy = 3,
    Enables = 4,
}
