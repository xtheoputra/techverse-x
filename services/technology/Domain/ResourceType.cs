namespace TechVerseX.TechnologyService.Domain;

/// <summary>
/// Jenis sumber belajar di bagian 5 template ADR-012.
/// </summary>
/// <remarks>
/// Keempat anggotanya datang apa adanya dari ADR-012 bagian 1: <em>dokumentasi
/// resmi, video, paper, repositori</em>. Ia sengaja <b>enum, bukan teks bebas</b>
/// — persis alasan yang ditulis ADR-015 untuk memilih tabel bertipe daripada
/// blok generik: bentuk yang ditegakkan tipe tidak bisa dilanggar diam-diam.
/// <para>
/// ⚠️ Menambah anggota baru berarti menambah jenis sumber yang boleh muncul di
/// halaman, dan itu keputusan template — lewat ADR, bukan lewat commit.
/// </para>
/// </remarks>
public enum ResourceType
{
    /// <summary>Dokumentasi resmi dari pembuat teknologinya sendiri.</summary>
    OfficialDocs = 0,

    /// <summary>Video — kuliah, konferensi, atau tutorial.</summary>
    Video = 1,

    /// <summary>Paper akademik. Sumber arXiv bermuara ke sini.</summary>
    Paper = 2,

    /// <summary>Repositori kode yang bisa dibaca dan dijalankan.</summary>
    Repository = 3,
}
