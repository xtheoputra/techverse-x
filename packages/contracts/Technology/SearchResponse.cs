using TechVerseX.Contracts.Common;

namespace TechVerseX.Contracts.Technology;

/// <summary>
/// Jawaban satu kali cari: <b>bidang dan topik sekaligus</b>.
/// </summary>
/// <remarks>
/// 🔑 <b>Kedua jenis entitas ada di sini karena keduanya punya halaman.</b>
/// ADR-009 memberi URL kanonik kepada <em>tiap bidang dan tiap topik</em>, dan
/// <c>/teknologi/&lt;slug&gt;</c> melayani keduanya. Hasil pencarian yang cuma
/// memuat topik akan menyembunyikan separuh halaman yang sebenarnya ada.
/// <para>
/// Dan selama isinya masih sedikit, yang tersembunyi bukan separuh melainkan
/// hampir seluruhnya: bidangnya jauh lebih banyak daripada topiknya. Pencarian
/// yang hanya menjawab topik akan membalas "tidak ada hasil" untuk hampir setiap
/// kata — termasuk kata yang tercetak di halaman muka.
/// </para>
/// <para>
/// ⚠️ <see cref="Fields"/> sengaja <b>tidak</b> berhalaman. Daftar bidang
/// tertutup (ADR-010) dan tidak bisa bertambah tanpa ADR dan migrasi;
/// memberi penomoran halaman pada daftar yang tidak bisa tumbuh hanya menambah
/// permukaan yang harus diuji. Topik bisa tumbuh, jadi ia berhalaman.
/// </para>
/// </remarks>
/// <param name="Query">
/// Kata kunci yang benar-benar dipakai server — sudah dipangkas dan dipotong ke
/// batas panjangnya. Ia dikirim balik supaya klien menampilkan apa yang DICARI,
/// bukan apa yang diketik; dua hal itu bisa berbeda.
/// </param>
/// <param name="Fields">Bidang yang cocok, dalam urutan tampil ADR-010.</param>
/// <param name="Technologies">Topik yang cocok, berperingkat.</param>
public sealed record SearchResponse(
    string Query,
    IReadOnlyList<FieldResponse> Fields,
    PagedResponse<TechnologySummaryResponse> Technologies)
{
    /// <summary>
    /// Tidak ada satu pun kecocokan di kedua sisi.
    /// </summary>
    /// <remarks>
    /// Diturunkan di kontrak, bukan dihitung klien: "kosong" di sini berarti
    /// <b>dua</b> daftar kosong, dan klien yang lupa memeriksa salah satunya akan
    /// menampilkan "tidak ada hasil" di atas daftar yang sebenarnya berisi.
    /// </remarks>
    public bool IsEmpty => Fields.Count == 0 && Technologies.Items.Count == 0;
}
