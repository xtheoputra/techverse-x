namespace TechVerseX.Contracts.Technology;

/// <summary>
/// Satu langkah roadmap — bagian 2 template ADR-012.
/// </summary>
/// <remarks>
/// 🔴 <b><c>Order</c> ada di RESPONS, tapi tidak ada di BADAN satu pun REQUEST</b>,
/// dan itu bentuk yang disengaja. Nomor langkah ditentukan agregat
/// (<c>AddRoadmapStep</c>), bukan dikirim pemanggil; kalau kontrak permintaannya
/// memuat nomor, roadmap berlubang berhenti jadi mustahil dan kembali jadi soal
/// ketelitian penulis. Satu-satunya tempat nomor muncul di sisi permintaan adalah
/// ALAMAT <c>PUT …/roadmap/{order}</c> (ADR-028 Tahap 2), dan di sana ia hanya
/// menunjuk langkah yang sudah ada.
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

/// <summary>
/// Satu media topik — gambar, diagram, atau video (ADR-028 Tahap 3b). Dirujuk dari teks
/// lewat <c>::media[Key]</c>; teks tak pernah memuat URL-nya.
/// </summary>
/// <remarks>
/// <c>Kind</c> adalah <c>Image</c> atau <c>Video</c>. Gambar membawa <c>Url</c> (berkas sendiri di
/// <c>/media/…</c>), video membawa <c>VideoId</c> (ID YouTube) — tak pernah keduanya.
/// <c>Alt</c> untuk video adalah judul bingkainya. <c>License</c> selalu ada; selain
/// <c>Karya sendiri</c>, <c>SourceName</c> dan <c>SourceUrl</c> juga.
/// </remarks>
public sealed record MediaResponse(
    string Key,
    string Kind,
    string? Url,
    string? VideoId,
    string Alt,
    string? Caption,
    string? SourceName,
    string? SourceUrl,
    string License);

/// <summary>
/// Muatan untuk menetapkan satu media (<c>PUT …/media/{key}</c>): menambah kalau kuncinya baru,
/// mengganti kalau sudah ada. Kunci ada di alamat, bukan di badan — sama dengan alat di
/// <c>…/tools/{toolSlug}</c>.
/// </summary>
public sealed record MediaRequest(
    string Kind,
    string? Url,
    string? VideoId,
    string Alt,
    string? Caption,
    string? SourceName,
    string? SourceUrl,
    string License);
