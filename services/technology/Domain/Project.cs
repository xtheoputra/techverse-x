namespace TechVerseX.TechnologyService.Domain;

/// <summary>
/// Mini Project — bagian 4 template ADR-012. Milik satu <see cref="Technology"/>.
/// </summary>
/// <remarks>
/// Berbeda dari <see cref="Tool"/> yang sengaja dipakai bersama, proyek memang
/// <b>milik satu topik</b> (ADR-015). "Bangun agen riset sederhana" hanya masuk
/// akal di halaman AI Agents; memakainya ulang di halaman lain akan mengubah
/// artinya.
/// <para>
/// Bukti selesai Bulan 6 di <c>RENCANA-V1.md</c> berbunyi <em>"satu proyek bisa
/// dikerjakan orang dari awal sampai selesai"</em>. Karena itu
/// <see cref="Brief"/> wajib — proyek tanpa penjelasan bukan proyek, ia judul.
/// </para>
/// </remarks>
public sealed class Project
{
    private Project()
    {
        // Dipakai EF Core.
        Title = string.Empty;
        Brief = string.Empty;
    }

    private Project(Guid id, Guid technologyId, string title, string brief)
    {
        Id = id;
        TechnologyId = technologyId;
        Title = title;
        Brief = brief;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid TechnologyId { get; private set; }

    public string Title { get; private set; }

    /// <summary>Apa yang dibangun dan kenapa. Wajib — lihat catatan kelas.</summary>
    public string Brief { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>Dipanggil <see cref="Technology"/> saja.</summary>
    internal static Project Create(Guid technologyId, string title, string brief)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(brief);

        return new Project(Guid.CreateVersion7(), technologyId, title.Trim(), brief.Trim());
    }
}
