namespace TechVerseX.Contracts.Technology;

/// <summary>
/// Satu langkah roadmap — bagian 2 template ADR-012.
/// </summary>
/// <remarks>
/// 🔴 <b><c>Order</c> ada di RESPONS, tapi tidak ada di satu pun REQUEST</b>, dan
/// itu bentuk yang disengaja. Nomor langkah ditentukan agregat
/// (<c>AddRoadmapStep</c>), bukan dikirim pemanggil; kalau kontrak permintaannya
/// memuat nomor, roadmap berlubang berhenti jadi mustahil dan kembali jadi soal
/// ketelitian penulis.
/// </remarks>
public sealed record RoadmapStepResponse(
    int Order,
    bool IsPrerequisite,
    string Title,
    string Description);

/// <summary>
/// Alat yang <b>ditautkan</b> ke sebuah topik — bagian 3 template ADR-012.
/// </summary>
/// <remarks>
/// Ia membawa identitas alat dari katalog (<c>Slug</c>, <c>Name</c>) plus
/// <c>Note</c> milik tautannya. Alatnya sendiri tidak disalin ke dalam topik:
/// dua topik yang memakai alat yang sama menunjuk baris katalog yang sama.
/// </remarks>
public sealed record TechnologyToolResponse(
    string Slug,
    string Name,
    string Summary,
    string? Homepage,
    string? Note);

/// <summary>Mini Project — bagian 4 template ADR-012.</summary>
public sealed record ProjectResponse(Guid Id, string Title, string Brief);

/// <summary>Sumber belajar — bagian 5 template ADR-012.</summary>
public sealed record ResourceResponse(Guid Id, string Type, string Title, string Url);

/// <summary>Satu alat di katalog, lepas dari topik mana pun.</summary>
public sealed record ToolResponse(Guid Id, string Slug, string Name, string Summary, string? Homepage);

/// <summary>Muatan untuk menambah satu alat ke katalog.</summary>
public sealed record CreateToolRequest(string Name, string Summary, string? Homepage = null, string? Slug = null);

/// <summary>
/// Muatan untuk langkah roadmap — dipakai prasyarat maupun langkah biasa.
/// </summary>
/// <remarks>
/// ⚠️ Sengaja <b>tidak</b> punya medan nomor. Lihat <see cref="RoadmapStepResponse"/>.
/// </remarks>
public sealed record RoadmapStepRequest(string Title, string Description);

/// <summary>Muatan untuk menautkan satu alat katalog ke sebuah topik.</summary>
public sealed record AttachToolRequest(string ToolSlug, string? Note = null);

/// <summary>Muatan untuk menambah Mini Project.</summary>
public sealed record ProjectRequest(string Title, string Brief);

/// <summary>
/// Muatan untuk menambah sumber belajar. <c>Type</c> harus salah satu anggota
/// <c>ResourceType</c> ADR-012: <c>OfficialDocs</c>, <c>Video</c>, <c>Paper</c>,
/// <c>Repository</c>.
/// </summary>
public sealed record ResourceRequest(string Type, string Title, string Url);
