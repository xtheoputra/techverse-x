namespace TechVerseX.Contracts.Technology;

/// <summary>
/// Muatan untuk menaikkan topik di alamat permintaan ke <c>tinjau</c>: kelima
/// bagiannya sudah <b>diperiksa manusia</b> (ADR-012, ADR-021).
/// </summary>
/// <remarks>
/// 🔑 <b><see cref="Reviewer"/> adalah nama manusia yang memikul pemeriksaan itu —
/// satu-satunya klaim kepercayaan produk ini.</b> Kontraknya memuatnya karena
/// <c>Technology.MarkReviewed</c> menuntutnya; ia tidak boleh teks yang diarang
/// pemanggil mana pun. Penjaganya BUKAN di kontrak ini melainkan di alur yang
/// memanggilnya: jalan produksi satu-satunya adalah workflow bergerbang ADR-021,
/// yang mengisi medan ini dari <c>github.actor</c> — nama yang sudah diautentikasi
/// GitHub — dan <b>tidak boleh punya masukan untuk menimpanya</b> (ADR-021 §2,
/// dijaga <c>TinjauWorkflowGuardTests</c>).
/// <para>
/// ⚠️ Endpoint tulisnya ikut hilang di produksi bersama permukaan tulis lain
/// (ADR-020). Di dev dan uji ia terbuka lewat <c>Editorial:WritesEnabled</c>, dua
/// tempat yang tidak pernah dilihat produksi.
/// </para>
/// </remarks>
/// <param name="Reviewer">Nama manusia yang memeriksa. Kosong ditolak 400 (domain menuntutnya).</param>
public sealed record MarkReviewedRequest(string Reviewer);
