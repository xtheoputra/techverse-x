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

    /// <summary>Lebar kolom <c>roadmap_steps.Title</c>; penjaga muatan dan konfigurasi EF memakai angka yang sama.</summary>
    public const int MaxTitleLength = 200;

    /// <summary>Lebar kolom <c>roadmap_steps.Description</c>.</summary>
    public const int MaxDescriptionLength = 2000;

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
        ArgumentOutOfRangeException.ThrowIfNegative(order);

        var (finalTitle, finalDescription) = Normalize(title, description);

        return new RoadmapStep(Guid.CreateVersion7(), technologyId, order, finalTitle, finalDescription);
    }

    /// <summary>
    /// Mengganti judul dan uraian langkah ini <b>di tempat</b> — nomornya tak berubah,
    /// dan barisnya tetap barisnya. Mengembalikan <c>true</c> kalau teks yang tersimpan
    /// benar-benar berbeda sesudah dipangkas.
    /// </summary>
    /// <remarks>
    /// Dipanggil <see cref="Technology"/> saja (nomor langkah dijaga agregat), dan
    /// jawabannya yang menentukan apakah pemeriksaan manusia gugur: mengulang teks yang
    /// sama bukan perubahan (ADR-012 Pembaruan 2026-10-06).
    /// </remarks>
    internal bool Rewrite(string title, string description)
    {
        var (finalTitle, finalDescription) = Normalize(title, description);

        var changed = Title != finalTitle || Description != finalDescription;
        Title = finalTitle;
        Description = finalDescription;
        return changed;
    }

    /// <summary>
    /// Memangkas dan menjaga lebar kolom. 🔴 Lebar dijaga DI SINI, bukan hanya di
    /// kolom basis data: teks yang kepanjangan sampai ke PostgreSQL sebagai galat
    /// server (500), padahal yang keliru muatannya — kelas cacat issue #26.
    /// </summary>
    private static (string Title, string Description) Normalize(string title, string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        var finalTitle = title.Trim();
        var finalDescription = description?.Trim() ?? string.Empty;

        if (finalTitle.Length > MaxTitleLength)
        {
            throw new ArgumentException($"Judul langkah maksimal {MaxTitleLength} karakter (diterima {finalTitle.Length}).", nameof(title));
        }

        if (finalDescription.Length > MaxDescriptionLength)
        {
            throw new ArgumentException($"Uraian langkah maksimal {MaxDescriptionLength} karakter (diterima {finalDescription.Length}).", nameof(description));
        }

        return (finalTitle, finalDescription);
    }
}
