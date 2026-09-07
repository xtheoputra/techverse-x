namespace TechVerseX.TechnologyService.Domain;

/// <summary>
/// Seberapa dalam sebuah bidang digarap. Tiga tingkat dari ADR-010.
/// </summary>
/// <remarks>
/// Prioritas ini bukan selera: ia menentukan <see cref="ContentMaturity"/> tertinggi
/// yang dijanjikan untuk topik di bawahnya, dan karena itu langsung menentukan
/// beban penulisan. Enam bidang <see cref="Core"/> memuat 42 topik yang dijanjikan
/// ditulis manusia; sisanya boleh berhenti di kurasi.
/// </remarks>
public enum FieldPriority
{
    /// <summary>Prioritas 1 — ditulis manusia sampai tuntas (<c>tinjau</c>).</summary>
    Core = 1,

    /// <summary>Prioritas 2 — lahir sebagai draf.</summary>
    Supporting = 2,

    /// <summary>
    /// Prioritas 3 — kurasi tautan saja, dan berhenti di situ sampai ada bukti
    /// permintaan. Ini keputusan sadar, bukan kelalaian: untuk XR, audit menilai
    /// relevansinya rendah dan uangnya sedang keluar dari sisi konsumen.
    /// </summary>
    Peripheral = 3,
}
