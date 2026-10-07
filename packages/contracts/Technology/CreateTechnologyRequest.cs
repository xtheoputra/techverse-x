namespace TechVerseX.Contracts.Technology;

/// <summary>
/// Muatan untuk membuat satu teknologi baru.
/// <c>Slug</c> dibiarkan kosong berarti diturunkan dari <c>Name</c>.
/// <c>FieldSlug</c> harus salah satu bidang di ADR-010 (daftar tertutup, lihat GET /api/v1/fields);
/// dulu kolom ini teks bebas bernama <c>Category</c>.
/// </summary>
public sealed record CreateTechnologyRequest(
    string Name,
    string Summary,
    string FieldSlug,
    string? Slug = null);
