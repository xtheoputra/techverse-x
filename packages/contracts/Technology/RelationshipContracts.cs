namespace TechVerseX.Contracts.Technology;

/// <summary>
/// Muatan untuk mencatat bahwa topik di alamat permintaan <b>membutuhkan</b>
/// <see cref="TopicSlug"/>: topik itu dipelajari lebih dulu (ADR-023).
/// </summary>
/// <remarks>
/// 🔑 <b>Tujuannya di MUATAN, bukan di alamat</b> — sama seperti
/// <see cref="AttachToolRequest"/>. Topik tujuan yang tidak ada jadi <b>400</b>
/// (muatannya keliru), bukan 404. Itu yang membuat gerbang ADR-020 tetap bisa
/// dibaca: dengan sakelar hidup handler membaca muatan dan membalas 400; dengan
/// sakelar mati tidak ada rute yang cocok dan jawabannya 404. Kalau tujuannya
/// segmen alamat, "topik tujuan tidak ada" dan "rutenya tidak dipasang" sama-sama
/// 404 dan tidak bisa dibedakan dari luar.
/// <para>
/// ⚠️ Tidak ada medan jenis relasi: V1 hanya punya <c>Requires</c>, dan satu rute
/// per jenis tidak perlu mengurai teks jenis sama sekali.
/// </para>
/// </remarks>
/// <param name="TopicSlug">Slug topik yang dibutuhkan. Harus topik, bukan bidang.</param>
public sealed record RequireTopicRequest(string TopicSlug);
