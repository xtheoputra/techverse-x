using TechVerseX.TechnologyService.Domain;

namespace TechVerseX.TechnologyService.Tests.Domain;

/// <summary>
/// Alat bantu uji: mengisi kelima bagian template ADR-012 sekaligus.
/// </summary>
/// <remarks>
/// Ia lahir karena <see cref="Technology.MarkDrafted"/> berhenti meluluskan
/// halaman kosong. Sebelum itu, "naikkan ke draf" cukup satu baris di uji mana
/// pun — dan itu justru yang membuat kalimat <em>"kelima bagian terisi"</em>
/// bertahan bertahun-tahun sebagai komentar yang tidak diperiksa siapa pun.
/// <para>
/// ⚠️ Kalau suatu saat menambah bagian keenam ke template, berkas ini yang
/// pertama memerah — dan itu memang gunanya.
/// </para>
/// </remarks>
internal static class TemplateLengkap
{
    /// <summary>Id alat contoh. Uji domain tidak menyentuh basis data, jadi ia tidak perlu ada di tabel mana pun.</summary>
    internal static readonly Guid AlatContoh = Guid.CreateVersion7();

    /// <summary>
    /// Mengisi Learning Roadmap, Tools, Mini Project, dan Resources. Overview
    /// sudah diisi <see cref="Technology.Create"/> lewat ringkasannya.
    /// </summary>
    internal static Technology IsiKelimaBagian(this Technology technology)
    {
        ArgumentNullException.ThrowIfNull(technology);

        // Langkah 0 lebih dulu — ADR-012 menjadikan prasyarat langkah pertama,
        // dan AddRoadmapStep menolak dipanggil sebelum ia ada.
        technology.SetPrerequisite("Prasyarat", "Python dasar dan satu panggilan HTTP.");
        technology.AddRoadmapStep("Langkah pertama", "Menjalankan contoh paling kecil.");

        technology.AttachTool(AlatContoh, "Dipakai di langkah pertama.");
        technology.AddProject("Proyek kecil", "Membangun satu hal yang jalan dari awal sampai selesai.");
        technology.AddResource(ResourceType.OfficialDocs, "Dokumentasi resmi", "https://example.com/docs");

        return technology;
    }
}
