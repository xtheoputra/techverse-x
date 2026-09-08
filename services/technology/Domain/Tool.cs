namespace TechVerseX.TechnologyService.Domain;

/// <summary>
/// Sebuah alat — bagian 3 template ADR-012. <b>Agregat sendiri, bukan anak
/// <see cref="Technology"/>.</b>
/// </summary>
/// <remarks>
/// ADR-015 menulis alasannya dan alasan itu yang membentuk kelas ini:
/// <em>satu tool dipakai lintas topik, dan menyalinnya per halaman menjamin data
/// yang saling bertentangan.</em> Kalau "LangGraph" ditulis ulang di lima
/// halaman, lima halaman itu akan berbeda pendapat soal apa LangGraph begitu
/// salah satunya disunting.
/// <para>
/// Karena itu <see cref="Technology"/> <b>tidak</b> memuat daftar
/// <see cref="Tool"/>; ia memuat daftar <see cref="TechnologyTool"/> —
/// tautannya, bukan salinannya.
/// </para>
/// </remarks>
public sealed class Tool
{
    /// <summary>Sama dengan <see cref="Technology.MaxSlugLength"/>, dan sengaja dinyatakan ulang di sini supaya lebar kolomnya punya satu sumber.</summary>
    public const int MaxSlugLength = 160;

    private Tool()
    {
        // Dipakai EF Core.
        Slug = string.Empty;
        Name = string.Empty;
        Summary = string.Empty;
    }

    private Tool(Guid id, string slug, string name, string summary, string? homepage)
    {
        Id = id;
        Slug = slug;
        Name = name;
        Summary = summary;
        Homepage = homepage;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    /// <summary>Kunci stabil alat ini. Unik lintas seluruh katalog.</summary>
    public string Slug { get; private set; }

    public string Name { get; private set; }

    public string Summary { get; private set; }

    /// <summary>Beranda resminya. Null kalau memang tidak punya.</summary>
    public string? Homepage { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public static Tool Create(string name, string summary, string? homepage = null, string? slug = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        // Slugs.From MELEMPAR untuk masukan yang tidak menyisakan karakter apa
        // pun ("!!!"). Itu disengaja dan sama dengan jalur Technology — lapisan
        // masukan yang bertugas mengubahnya jadi 400, bukan kelas ini.
        var finalSlug = string.IsNullOrWhiteSpace(slug) ? Slugs.From(name) : Slugs.From(slug);

        if (finalSlug.Length > MaxSlugLength)
        {
            throw new ArgumentException(
                $"Slug turunan '{finalSlug}' melebihi {MaxSlugLength} karakter.", nameof(name));
        }

        return new Tool(
            Guid.CreateVersion7(),
            finalSlug,
            name.Trim(),
            summary?.Trim() ?? string.Empty,
            string.IsNullOrWhiteSpace(homepage) ? null : homepage.Trim());
    }
}
