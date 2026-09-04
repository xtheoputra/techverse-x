namespace TechVerseX.TechnologyService.Domain;

/// <summary>
/// Seberapa dipercaya ISI sebuah halaman. Tiga tingkat dari ADR-012:
/// <c>kurasi</c> · <c>draf</c> · <c>tinjau</c>.
/// </summary>
/// <remarks>
/// 🔴 <b>Ini BUKAN <see cref="TechnologyStatus"/>, dan keduanya tidak boleh
/// digabung</b> (ADR-015 bagian 1). Dua sumbu yang berbeda:
/// <list type="bullet">
///   <item><see cref="TechnologyStatus"/> menjawab <b>boleh tayang?</b></item>
///   <item><see cref="ContentMaturity"/> menjawab <b>seberapa dipercaya isinya?</b></item>
/// </list>
/// Keduanya bersilangan secara sah: <see cref="Curated"/> yang sudah
/// <see cref="TechnologyStatus.Published"/> adalah bentuk akhir yang BENAR untuk
/// bidang prioritas 3, dan <see cref="MachineDrafted"/> yang sudah terbit adalah
/// keadaan normal — asalkan berlabel.
/// <para>
/// Kalau suatu saat keduanya terasa mirip dan tergoda disatukan: yang hilang
/// adalah kemampuan menerbitkan halaman kurasi yang jujur. Jangan disatukan.
/// </para>
/// </remarks>
public enum ContentMaturity
{
    /// <summary>
    /// <c>kurasi</c> — ringkasan singkat plus tautan terpilih, tanpa klaim panjang.
    /// Manusia memilih tautannya. Ini <b>bentuk akhir yang sah</b>, bukan utang.
    /// </summary>
    Curated = 0,

    /// <summary>
    /// <c>draf</c> — kelima bagian template terisi, tapi <b>belum diperiksa manusia</b>.
    /// Boleh tayang, tapi wajib berlabel — lihat ADR-012.
    /// </summary>
    MachineDrafted = 1,

    /// <summary>
    /// <c>tinjau</c> — kelima bagian terisi <b>dan sudah diperiksa manusia</b>.
    /// Satu-satunya jalan ke sini adalah <see cref="Technology.MarkReviewed"/>,
    /// yang menuntut nama pemeriksanya.
    /// </summary>
    HumanReviewed = 2,
}
