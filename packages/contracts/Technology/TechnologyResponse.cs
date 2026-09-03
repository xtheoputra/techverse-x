namespace TechVerseX.Contracts.Technology;

/// <summary>
/// Satu teknologi, bentuk lengkap.
/// </summary>
/// <remarks>
/// ⚠️ Bidang di sini SENGAJA sedikit. Skema basis data penuh masih Issue #20,
/// dan taksonomi bidang/submenu masih Issue #1 dan #3. Yang dimuat baru bidang
/// yang tidak bergantung pada ketiga keputusan itu.
/// </remarks>
public sealed record TechnologyResponse(
    Guid Id,
    string Slug,
    string Name,
    string Summary,
    string Category,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

/// <summary>
/// Bentuk ringkas untuk daftar dan hasil pencarian.
/// </summary>
public sealed record TechnologySummaryResponse(
    Guid Id,
    string Slug,
    string Name,
    string Summary,
    string Category);
