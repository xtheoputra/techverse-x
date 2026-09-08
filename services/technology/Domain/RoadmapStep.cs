namespace TechVerseX.TechnologyService.Domain;

/// <summary>
/// Satu langkah di <b>Learning Roadmap</b> — bagian 2 template ADR-012.
/// </summary>
/// <remarks>
/// 🔑 <b>Langkah 0 adalah prasyarat.</b> ADR-012 memilih Versi B justru karena
/// itu: <em>skill prerequisite</em> bukan bagian terpisah, ia langkah pertama
/// belajar. Menaruhnya di bagian sendiri membuat pembaca membacanya dua kali dan
/// penulis menulisnya dua kali.
/// <para>
/// <see cref="Order"/> <b>tidak pernah diisi pemanggil.</b> Ia diberikan
/// <see cref="Technology"/> secara berurutan — lihat
/// <see cref="Technology.SetPrerequisite"/> dan
/// <see cref="Technology.AddRoadmapStep"/>. Alasannya sama dengan alasan ADR-015
/// menolak blok generik: kalau nomor langkah boleh dikirim dari luar, roadmap
/// berlubang (0, 1, 4) menjadi mungkin, dan "langkah 0 adalah prasyarat" turun
/// pangkat jadi komentar. Di sini ia tidak bisa dilanggar diam-diam.
/// </para>
/// </remarks>
public sealed class RoadmapStep
{
    /// <summary>Nomor langkah prasyarat. Bukan angka ajaib — ADR-012 menamainya.</summary>
    public const int PrerequisiteOrder = 0;

    private RoadmapStep()
    {
        // Dipakai EF Core.
        Title = string.Empty;
        Description = string.Empty;
    }

    private RoadmapStep(Guid id, Guid technologyId, int order, string title, string description)
    {
        Id = id;
        TechnologyId = technologyId;
        Order = order;
        Title = title;
        Description = description;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid TechnologyId { get; private set; }

    /// <summary>Urutan langkah, mulai dari <see cref="PrerequisiteOrder"/>.</summary>
    public int Order { get; private set; }

    public string Title { get; private set; }

    public string Description { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>Apakah langkah ini prasyaratnya — pertanyaan yang dijawab nomor, bukan sebuah bendera tersendiri.</summary>
    public bool IsPrerequisite => Order == PrerequisiteOrder;

    /// <summary>
    /// Dipanggil <see cref="Technology"/> saja. <c>internal</c> supaya nomor
    /// urutnya tidak bisa ditentukan dari luar agregat.
    /// </summary>
    internal static RoadmapStep Create(Guid technologyId, int order, string title, string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentOutOfRangeException.ThrowIfNegative(order);

        return new RoadmapStep(
            Guid.CreateVersion7(),
            technologyId,
            order,
            title.Trim(),
            description?.Trim() ?? string.Empty);
    }
}
