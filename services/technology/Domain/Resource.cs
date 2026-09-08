namespace TechVerseX.TechnologyService.Domain;

/// <summary>
/// Satu sumber belajar — bagian 5 template ADR-012. Milik satu <see cref="Technology"/>.
/// </summary>
/// <remarks>
/// 🔑 <b>Bagian inilah yang membuat halaman tetap utuh tanpa pipeline berita.</b>
/// ADR-012 memilih Versi B persis karena Versi A menjadikan "Berita terbaru" dan
/// "Paper terbaru" bagian wajib — yang berarti kelengkapan 82 halaman menggantung
/// pada Issue #17, pertanyaan hukum yang hanya bisa dijawab pemilik. Di Versi B,
/// paper cuma salah satu <see cref="ResourceType"/>, dan halaman tetap lengkap
/// tanpa satu pun.
/// </remarks>
public sealed class Resource
{
    private Resource()
    {
        // Dipakai EF Core.
        Title = string.Empty;
        Url = string.Empty;
    }

    private Resource(Guid id, Guid technologyId, ResourceType type, string title, string url)
    {
        Id = id;
        TechnologyId = technologyId;
        Type = type;
        Title = title;
        Url = url;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid TechnologyId { get; private set; }

    public ResourceType Type { get; private set; }

    public string Title { get; private set; }

    /// <summary>Alamat sumbernya. Wajib absolut — sumber belajar yang tidak bisa dibuka bukan sumber belajar.</summary>
    public string Url { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>Dipanggil <see cref="Technology"/> saja.</summary>
    internal static Resource Create(Guid technologyId, ResourceType type, string title, string url)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(url);

        // Absolut, dan hanya http/https. Tautan relatif akan lolos ke halaman
        // lalu mati di peramban pembaca; skema lain (javascript:, file:) tidak
        // punya urusan di daftar sumber belajar.
        if (!Uri.TryCreate(url.Trim(), UriKind.Absolute, out var parsed)
            || (parsed.Scheme != Uri.UriSchemeHttp && parsed.Scheme != Uri.UriSchemeHttps))
        {
            throw new ArgumentException(
                $"Sumber '{title}' harus punya URL http/https absolut, bukan '{url}'.", nameof(url));
        }

        return new Resource(Guid.CreateVersion7(), technologyId, type, title.Trim(), parsed.ToString());
    }
}
