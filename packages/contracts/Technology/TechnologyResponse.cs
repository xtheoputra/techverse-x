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
/// <para>
/// <b><see cref="Requires"/> dan <see cref="RequiredBy"/> — relasi antar-topik
/// (ADR-023) — BUKAN bagian template.</b> Keduanya tidak pernah dihitung
/// <see cref="MissingSections"/>: halaman tanpa satu relasi pun tetap lengkap.
/// </para>
/// <para>
/// 🔑 <b>Arah kebalikannya diturunkan server, sama seperti MissingSections.</b>
/// Yang tersimpan hanya satu baris "A membutuhkan B"; <see cref="RequiredBy"/> di
/// halaman B dihitung saat dibaca. Klien tidak pernah menyusun ulang kebalikannya
/// sendiri — dan tidak ada baris kedua yang bisa menyimpang dari yang pertama.
/// </para>
/// <para>
/// 🔴 Tiap butirnya <see cref="TechnologySummaryResponse"/>, bukan tipe baru,
/// supaya judul topik yang ditautkan tidak pernah bisa bepergian tanpa label
/// kematangannya (ADR-012). Aturan itu dijaga tipe, bukan ingatan penulis UI.
/// </para>
/// </remarks>
/// <param name="Requires">
/// Topik yang dibutuhkan topik ini — "pelajari lebih dulu". Urut menurut urutan
/// tampil bidang, lalu nama.
/// </param>
/// <param name="RequiredBy">
/// Topik yang membutuhkan topik ini — "dibutuhkan oleh". Urutannya sama.
/// </param>
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
    IReadOnlyList<string> MissingSections,
    IReadOnlyList<TechnologySummaryResponse> Requires,
    IReadOnlyList<TechnologySummaryResponse> RequiredBy);

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
