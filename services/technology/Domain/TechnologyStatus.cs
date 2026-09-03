namespace TechVerseX.TechnologyService.Domain;

/// <summary>
/// Daur hidup satu entri teknologi.
/// Rujukan: KERANGKA.md 4.6 Research Pipeline — sebuah entri berjalan dari
/// ditemukan → diverifikasi → terbit. Entri yang ditemukan agen TIDAK boleh
/// langsung terbit tanpa melewati verifikasi.
/// </summary>
public enum TechnologyStatus
{
    /// <summary>Ditulis manusia, belum siap tayang.</summary>
    Draft = 0,

    /// <summary>Ditemukan Research Agent, belum diperiksa siapa pun.</summary>
    Discovered = 1,

    /// <summary>Sudah lolos verifikasi sumber.</summary>
    Verified = 2,

    /// <summary>Tayang untuk pembaca.</summary>
    Published = 3,

    /// <summary>Tidak lagi relevan, disimpan sebagai rekaman.</summary>
    Archived = 4,
}
