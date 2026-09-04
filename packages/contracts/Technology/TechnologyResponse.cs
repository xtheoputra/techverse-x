namespace TechVerseX.Contracts.Technology;

/// <summary>
/// Satu teknologi, bentuk lengkap.
/// </summary>
/// <remarks>
/// Isi halaman menurut template ADR-012 — roadmap, tools, mini project,
/// resources — belum ada di sini; itu irisan berikutnya.
/// </remarks>
public sealed record TechnologyResponse(
    Guid Id,
    string Slug,
    string Name,
    string Summary,
    string FieldSlug,
    string FieldName,
    string Status,
    string Maturity,
    DateTimeOffset? ReviewedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

/// <summary>
/// Bentuk ringkas untuk daftar dan hasil pencarian.
/// </summary>
/// <remarks>
/// 🔴 <b><c>Maturity</c> ada di bentuk ringkas ini dengan sengaja</b>, meski daftar
/// tidak menampilkan isi halaman. ADR-012 mewajibkan halaman yang belum diperiksa
/// manusia selalu berlabel; kalau kontrak daftar boleh tidak membawa kematangan,
/// maka ada jalan menampilkan judulnya tanpa labelnya — dan aturan itu jadi
/// bergantung pada ingatan penulis UI. Di sini ia bergantung pada tipe.
/// </remarks>
public sealed record TechnologySummaryResponse(
    Guid Id,
    string Slug,
    string Name,
    string Summary,
    string FieldSlug,
    string FieldName,
    string Maturity);
