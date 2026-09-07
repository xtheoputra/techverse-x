using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace TechVerseX.TechnologyService.Domain;

/// <summary>
/// Akar agregat bidang Technology — "jantung TechVerse X" (KERANGKA.md 4.6).
/// </summary>
/// <remarks>
/// ⚠️ Sengaja MINIM. Skema penuh (Concept, Tool, Framework, Company, Skill,
/// Project, Resource, Timeline) masih Issue #20, dan taksonominya Issue #1/#3.
/// Yang dimodelkan di sini hanya yang tidak bergantung pada keputusan itu.
/// </remarks>
public sealed partial class Technology
{
    private readonly List<DomainEvent> _events = [];
    private readonly List<TechnologyRelationship> _relationships = [];

    private Technology()
    {
        // Dipakai EF Core.
        Slug = string.Empty;
        Name = string.Empty;
        Summary = string.Empty;
        Category = string.Empty;
    }

    private Technology(Guid id, string slug, string name, string summary, string category)
    {
        Id = id;
        Slug = slug;
        Name = name;
        Summary = summary;
        Category = category;
        Status = TechnologyStatus.Draft;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public Guid Id { get; private set; }

    /// <summary>Kunci yang dipakai di URL — lihat KERANGKA.md 4.9 <c>/api/v1/technologies/{slug}</c>.</summary>
    public string Slug { get; private set; }

    public string Name { get; private set; }

    public string Summary { get; private set; }

    /// <summary>
    /// Untuk sementara berupa teks bebas, BUKAN kunci asing.
    /// Taksonomi bidang masih Issue #1 dan #3; memaksakan tabel kategori
    /// sekarang berarti menebak keputusan yang belum diambil.
    /// </summary>
    public string Category { get; private set; }

    public TechnologyStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public IReadOnlyList<TechnologyRelationship> Relationships => _relationships;

    public IReadOnlyList<DomainEvent> Events => _events;

    public static Technology Create(string name, string summary, string category, string? slug = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(category);

        var finalSlug = string.IsNullOrWhiteSpace(slug) ? Slugify(name) : Slugify(slug);

        // Guid v7 berurut menurut waktu — indeks primer tidak terfragmentasi
        // seperti kalau memakai Guid acak.
        var technology = new Technology(Guid.CreateVersion7(), finalSlug, name.Trim(), summary?.Trim() ?? string.Empty, category.Trim());
        technology._events.Add(new TechnologyCreated(technology.Id, technology.Slug, technology.Name));
        return technology;
    }

    public void Update(string name, string summary, string category)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(category);

        Name = name.Trim();
        Summary = summary?.Trim() ?? string.Empty;
        Category = category.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
        _events.Add(new TechnologyUpdated(Id, Slug));
    }

    /// <summary>
    /// Menerbitkan entri. Sengaja MENOLAK entri yang baru ditemukan agen dan
    /// belum diverifikasi — lihat pipeline di KERANGKA.md 4.6, yang menaruh
    /// Verification sebelum Publish justru supaya halusinasi tidak ikut tayang.
    /// </summary>
    public void Publish()
    {
        if (Status == TechnologyStatus.Discovered)
        {
            throw new InvalidOperationException(
                $"Teknologi '{Slug}' baru berstatus Discovered. Ia harus lolos verifikasi sumber dulu sebelum boleh terbit.");
        }

        if (Status == TechnologyStatus.Published)
        {
            return;
        }

        Status = TechnologyStatus.Published;
        UpdatedAt = DateTimeOffset.UtcNow;
        _events.Add(new TechnologyPublished(Id, Slug));
    }

    public void ClearEvents() => _events.Clear();

    /// <summary>Mengubah "AI Agents" menjadi "ai-agents".</summary>
    public static string Slugify(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var normalized = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var ch in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(ch);
            }
        }

        var withoutMarks = builder.ToString().Normalize(NormalizationForm.FormC);
        var slug = NonSlugCharacters().Replace(withoutMarks, "-");
        slug = CollapsedDashes().Replace(slug, "-").Trim('-');

        return slug.Length == 0
            ? throw new ArgumentException($"'{value}' tidak menyisakan satu karakter pun yang bisa dipakai sebagai slug.", nameof(value))
            : slug;
    }

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NonSlugCharacters();

    [GeneratedRegex("-{2,}")]
    private static partial Regex CollapsedDashes();
}
