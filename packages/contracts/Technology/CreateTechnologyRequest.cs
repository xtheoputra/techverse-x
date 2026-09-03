namespace TechVerseX.Contracts.Technology;

/// <summary>
/// Muatan untuk membuat satu teknologi baru.
/// <c>Slug</c> dibiarkan kosong berarti diturunkan dari <c>Name</c>.
/// </summary>
public sealed record CreateTechnologyRequest(
    string Name,
    string Summary,
    string Category,
    string? Slug = null);
