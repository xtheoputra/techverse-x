namespace TechVerseX.TechnologyService.Domain;

/// <summary>
/// Jenis hubungan antara dua topik. <b>V1 hanya punya satu: <see cref="Requires"/></b>
/// (ADR-023).
/// </summary>
/// <remarks>
/// 🔑 Dulu enum ini diambil apa adanya dari graph model KERANGKA.md 4.6 —
/// <c>requires</c>, <c>uses</c>, <c>related</c>, <c>used_by</c>, <c>enables</c> —
/// dan tidak satu pun pernah dipakai satu sisi sungguhan. Diperiksa terhadap ADR
/// yang diterima, jenis-jenis lain itu menunjuk sesuatu yang BUKAN topik V1:
/// sebuah Skill, sebuah Company (bukan entitas V1, ADR-015), sebuah peran
/// (dibuang ADR-009/010), atau katalog Tool (<see cref="TechnologyTool"/>).
/// <c>UsedBy</c> dan <c>Enables</c> pun cuma kebalikan dari jenis lain, jadi satu
/// fakta bisa tersimpan dua cara.
/// <para>
/// ⚠️ <b>Menambah jenis BUKAN sekadar menambah baris di sini.</b> Ia menuntut ADR,
/// migrasi — CHECK <c>ck_technology_relationships_kind</c> dibangkitkan dari nama
/// anggota enum ini, jadi menambah anggota langsung membuat model EF berbeda dari
/// snapshot — dan label tampilan di web. Membuang jenis setelah ada barisnya jauh
/// lebih mahal: butuh migrasi data, dan sampai itu ada, halaman yang memuat sisi
/// berjenis hilang akan galat.
/// </para>
/// <para>
/// Kind disimpan sebagai TEKS, jadi angka <c>0</c> di bawah tidak berarti apa pun
/// bagi basis data.
/// </para>
/// </remarks>
public enum RelationshipKind
{
    /// <summary>Topik asal membutuhkan topik tujuan: pelajari tujuannya lebih dulu.</summary>
    Requires = 0,
}
