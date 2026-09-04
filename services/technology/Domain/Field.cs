namespace TechVerseX.TechnologyService.Domain;

/// <summary>
/// Satu bidang teknologi. Empat belas di antaranya, ditetapkan ADR-010.
/// </summary>
/// <remarks>
/// Sebelum ini <c>Technology.Category</c> berupa teks bebas, karena taksonominya
/// belum diputuskan (Issue #1 dan #3). Keduanya sudah ditutup, jadi kategori naik
/// jadi entitas dengan kunci asing — dan pindahnya dilakukan SEKARANG, saat isinya
/// baru beberapa baris contoh (ADR-015).
/// <para>
/// Daftar isinya tidak dibuat lewat API: ia <see cref="FieldCatalog"/>, disemai
/// migrasi. Bidang bukan data pengguna, melainkan bagian dari keputusan struktur —
/// menambah atau membuang satu bidang harus lewat ADR, bukan lewat POST.
/// </para>
/// </remarks>
public sealed class Field
{
    private Field()
    {
        // Dipakai EF Core.
        Slug = string.Empty;
        Name = string.Empty;
        Summary = string.Empty;
    }

    private Field(Guid id, string slug, string name, string summary, FieldPriority priority, int displayOrder)
    {
        Id = id;
        Slug = slug;
        Name = name;
        Summary = summary;
        Priority = priority;
        DisplayOrder = displayOrder;
    }

    public Guid Id { get; private set; }

    /// <summary>Bagian bidang dari URL kanonik ADR-009.</summary>
    public string Slug { get; private set; }

    public string Name { get; private set; }

    public string Summary { get; private set; }

    public FieldPriority Priority { get; private set; }

    /// <summary>Urutan tampil di menu. Bukan urutan abjad — prioritas dulu.</summary>
    public int DisplayOrder { get; private set; }

    /// <summary>
    /// Membuat satu bidang. <c>internal</c> dengan sengaja: satu-satunya pemanggil
    /// yang sah adalah <see cref="FieldCatalog"/>.
    /// </summary>
    internal static Field Create(Guid id, string name, string summary, FieldPriority priority, int displayOrder, string? slug = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (displayOrder < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(displayOrder), displayOrder, "Urutan tampil dimulai dari 1.");
        }

        var finalSlug = string.IsNullOrWhiteSpace(slug) ? Slugs.From(name) : Slugs.From(slug);

        return new Field(id, finalSlug, name.Trim(), summary?.Trim() ?? string.Empty, priority, displayOrder);
    }
}
