namespace TechVerseX.Contracts.Technology;

/// <summary>
/// Muatan untuk <b>mengganti</b> nama dan ringkasan sebuah topik (ADR-028 Tahap 2, #79).
/// </summary>
/// <remarks>
/// 🔑 <b>PUT, jadi dua-duanya wajib dan mengganti seluruhnya</b> — bukan tambal.
/// <c>Slug</c> dan <c>FieldSlug</c> sengaja tidak ada: slug adalah alamat halaman yang
/// sudah bisa ditautkan orang, dan bidangnya soal taksonomi (ADR-009, ADR-010), bukan
/// soal menyunting tulisan.
/// <para>
/// ⚠️ Pada topik yang sudah <c>tinjau</c>, isi yang <b>berbeda</b> menggugurkannya ke
/// <c>draf</c>; mengulang isi yang sama tidak (ADR-012 Pembaruan 2026-10-06).
/// </para>
/// <para>
/// 🔑 <c>Overview</c> (Markdown terbatas, ADR-028 Tahap 3b) ikut diganti seluruhnya: <b>tidak
/// dikirim berarti dikosongkan</b>, seperti semua PUT di sini. Klien yang hanya mengubah nama
/// harus mengirim kembali overview yang ada.
/// </para>
/// </remarks>
public sealed record UpdateTechnologyRequest(string Name, string Summary, string? Overview = null);

/// <summary>
/// Muatan untuk <b>mengganti</b> sebuah alat di katalog. <c>Slug</c> tak bisa diubah —
/// ia identitas yang dirujuk berkas isi dan tautan topik.
/// </summary>
/// <remarks>
/// Alat dipakai bersama banyak topik, jadi mengganti <c>Name</c> atau <c>Summary</c>
/// menggugurkan pemeriksaan <b>setiap</b> topik <c>tinjau</c> yang menautkannya.
/// <c>Homepage</c> tautan, bukan teks: mengubahnya tidak menggugurkan apa pun.
/// </remarks>
public sealed record UpdateToolRequest(string Name, string Summary, string? Homepage = null);
