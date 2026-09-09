namespace TechVerseX.Contracts.Technology;

/// <summary>
/// Satu teknologi, bentuk lengkap — <b>termasuk kelima bagian template ADR-012</b>.
/// </summary>
/// <remarks>
/// Bagian 1 template adalah <see cref="Summary"/> sendiri (<em>Overview</em>);
/// empat sisanya dibawa keempat daftar di bawah.
/// <para>
/// 🔑 <b><see cref="MissingSections"/> ikut dikirim, dan itu bukan kemewahan.</b>
/// Ia jawaban <c>Technology.MissingSections</c> apa adanya: bagian mana yang masih
/// kosong, memakai istilah template. Tanpa medan ini, satu-satunya cara klien tahu
/// sebuah halaman belum lengkap adalah menghitung sendiri keempat daftar itu — dan
/// aturan <em>"roadmap butuh langkah SESUDAH langkah 0"</em> akan ditulis ulang di
/// setiap klien, lalu menyimpang di salah satunya.
/// </para>
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
    DateTimeOffset UpdatedAt,
    IReadOnlyList<RoadmapStepResponse> Roadmap,
    IReadOnlyList<TechnologyToolResponse> Tools,
    IReadOnlyList<ProjectResponse> Projects,
    IReadOnlyList<ResourceResponse> Resources,
    IReadOnlyList<string> MissingSections);

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
