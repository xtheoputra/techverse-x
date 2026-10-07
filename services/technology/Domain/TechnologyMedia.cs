using System.Text.RegularExpressions;

namespace TechVerseX.TechnologyService.Domain;

/// <summary>
/// Satu gambar, diagram, atau video milik sebuah topik — ADR-028 Tahap 3b. Anak
/// <see cref="Technology"/>, seperti <see cref="Resource"/>: tak punya arti di luar topiknya.
/// </summary>
/// <remarks>
/// 🔑 <b>Kuncinya (<see cref="Key"/>) adalah alamat dari teks.</b> Isi berformat menaruh media
/// dengan blok <c>::media[kunci]</c> sendirian di satu baris, jadi satu media bisa dipakai di
/// bagian mana pun tanpa menyalin datanya, dan teks tak pernah memuat URL gambar. Karena itu
/// kunci unik per topik dan berbentuk slug.
/// <para>
/// 🔴 <b>Setiap media punya teks alternatif dan sumber yang bisa diperiksa mesin</b> — itu
/// alasan media tak boleh lewat sintaks gambar Markdown (ADR-028 butir 3). Aturannya ditegakkan
/// DI SINI dan diulang sebagai <c>CHECK</c> di basis data, bukan hanya di pemeriksa berkas:
/// <list type="bullet">
/// <item><see cref="Alt"/> wajib (untuk video: judul bingkainya — pembaca layar menyebutnya).</item>
/// <item><see cref="License"/> wajib; kecuali <see cref="OwnWork"/>, sumber (<see cref="SourceName"/> dan
/// <see cref="SourceUrl"/>) wajib juga — gambar tanpa asal-usul tak boleh tayang.</item>
/// <item>Gambar hanya boleh berupa BERKAS SENDIRI di <c>/media/…</c> (lisensinya dicatat dan
/// berkasnya tak bisa hilang diam-diam di hulu); tak ada gambar hotlink.</item>
/// </list>
/// </para>
/// <para>
/// Lebar kolom dijaga di sini supaya teks yang kepanjangan jadi 400 bermedan, bukan 500 dari
/// PostgreSQL (kelas cacat #26). Nama parameter sengaja sama persis dengan medan muatan
/// (<c>videoId</c>, <c>sourceUrl</c>) — penerjemah galat memakai <c>ParamName</c> apa adanya.
/// </para>
/// </remarks>
public sealed partial class TechnologyMedia
{
    /// <summary>Nilai <see cref="License"/> untuk karya yang digambar sendiri; satu-satunya yang membebaskan sumber.</summary>
    public const string OwnWork = "Karya sendiri";

    public const int MaxKeyLength = 80;
    public const int MaxUrlLength = 300;
    public const int MaxAltLength = 500;
    public const int MaxCaptionLength = 1000;
    public const int MaxSourceNameLength = 200;
    public const int MaxSourceUrlLength = 1000;
    public const int MaxLicenseLength = 200;
    public const int VideoIdLength = 11;

    private TechnologyMedia()
    {
        // Dipakai EF Core.
        Key = string.Empty;
        Alt = string.Empty;
        License = string.Empty;
    }

    private TechnologyMedia(
        Guid id, Guid technologyId, string key, MediaKind kind, string? url, string? videoId,
        string alt, string? caption, string? sourceName, string? sourceUrl, string license)
    {
        Id = id;
        TechnologyId = technologyId;
        Key = key;
        Kind = kind;
        Url = url;
        VideoId = videoId;
        Alt = alt;
        Caption = caption;
        SourceName = sourceName;
        SourceUrl = sourceUrl;
        License = license;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid TechnologyId { get; private set; }

    /// <summary>Alamat dari teks (<c>::media[kunci]</c>). Slug, unik per topik, tak pernah berubah.</summary>
    public string Key { get; private set; }

    public MediaKind Kind { get; private set; }

    /// <summary>Hanya <see cref="MediaKind.Image"/>: jalur berkas sendiri, selalu diawali <c>/media/</c>.</summary>
    public string? Url { get; private set; }

    /// <summary>Hanya <see cref="MediaKind.Video"/>: ID YouTube sebelas karakter.</summary>
    public string? VideoId { get; private set; }

    /// <summary>Teks alternatif gambar; untuk video, judul bingkainya.</summary>
    public string Alt { get; private set; }

    public string? Caption { get; private set; }

    public string? SourceName { get; private set; }

    public string? SourceUrl { get; private set; }

    public string License { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>Dipanggil <see cref="Technology"/> saja: kunci stabil dan teks yang sudah divalidasi.</summary>
    internal static TechnologyMedia Create(
        Guid technologyId, string key, MediaKind kind, string? url, string? videoId,
        string alt, string? caption, string? sourceName, string? sourceUrl, string license)
    {
        var n = Normalize(key, kind, url, videoId, alt, caption, sourceName, sourceUrl, license);
        return new TechnologyMedia(Guid.CreateVersion7(), technologyId, n.Key, n.Kind, n.Url, n.VideoId, n.Alt, n.Caption, n.SourceName, n.SourceUrl, n.License);
    }

    /// <summary>
    /// Mengganti isi di tempat (kuncinya, dan barisnya, tetap). <c>true</c> kalau ADA yang
    /// berbeda sesudah dinormalisasi — termasuk gambar atau ID video yang diganti: pemeriksa
    /// menyetujui teks alternatif untuk GAMBAR ITU, bukan untuk gambar apa pun di kunci yang sama.
    /// </summary>
    internal bool Rewrite(
        MediaKind kind, string? url, string? videoId,
        string alt, string? caption, string? sourceName, string? sourceUrl, string license)
    {
        var n = Normalize(Key, kind, url, videoId, alt, caption, sourceName, sourceUrl, license);

        var changed = Kind != n.Kind || Url != n.Url || VideoId != n.VideoId || Alt != n.Alt
            || Caption != n.Caption || SourceName != n.SourceName || SourceUrl != n.SourceUrl || License != n.License;

        Kind = n.Kind;
        Url = n.Url;
        VideoId = n.VideoId;
        Alt = n.Alt;
        Caption = n.Caption;
        SourceName = n.SourceName;
        SourceUrl = n.SourceUrl;
        License = n.License;
        return changed;
    }

    private static (string Key, MediaKind Kind, string? Url, string? VideoId, string Alt, string? Caption, string? SourceName, string? SourceUrl, string License)
        Normalize(string key, MediaKind kind, string? url, string? videoId, string alt, string? caption, string? sourceName, string? sourceUrl, string license)
    {
        var finalKey = key?.Trim() ?? string.Empty;
        if (finalKey.Length == 0 || finalKey.Length > MaxKeyLength || !KeyPattern().IsMatch(finalKey))
        {
            throw new ArgumentException(
                $"Kunci media '{key}' harus slug (huruf kecil, angka, tanda hubung tunggal), maksimal {MaxKeyLength} karakter.", nameof(key));
        }

        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentException($"Jenis media tak dikenal: {kind}.", nameof(kind));
        }

        string? finalUrl = null;
        string? finalVideoId = null;

        if (kind == MediaKind.Image)
        {
            if (!string.IsNullOrWhiteSpace(videoId))
            {
                throw new ArgumentException("Gambar tak punya videoId.", nameof(videoId));
            }

            finalUrl = url?.Trim();
            if (string.IsNullOrEmpty(finalUrl))
            {
                throw new ArgumentException("Gambar wajib punya url (berkas sendiri di /media/…).", nameof(url));
            }

            // Hanya BERKAS SENDIRI: jalur absolut di bawah /media/, berekstensi gambar, tanpa
            // naik direktori. Gambar hotlink dari situs lain tak punya lisensi yang kita pegang
            // dan bisa hilang atau berganti isi kapan saja.
            if (finalUrl.Length > MaxUrlLength || !OwnFilePattern().IsMatch(finalUrl) || finalUrl.Contains("..", StringComparison.Ordinal) || finalUrl.Contains("//", StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    $"url gambar harus jalur berkas sendiri di /media/ berekstensi svg, png, jpg, jpeg, webp, atau avif (diterima: '{url}').", nameof(url));
            }
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException("Video tak punya url; pakai videoId.", nameof(url));
            }

            finalVideoId = videoId?.Trim();
            if (finalVideoId is null || finalVideoId.Length != VideoIdLength || !VideoIdPattern().IsMatch(finalVideoId))
            {
                throw new ArgumentException($"videoId harus ID YouTube {VideoIdLength} karakter (huruf, angka, _ dan -), bukan '{videoId}'.", nameof(videoId));
            }
        }

        var finalAlt = alt?.Trim() ?? string.Empty;
        if (finalAlt.Length == 0)
        {
            throw new ArgumentException(
                kind == MediaKind.Image ? "Teks alternatif (alt) wajib: media tanpa teks alternatif tak boleh tayang." : "Judul bingkai video (alt) wajib.", nameof(alt));
        }

        if (finalAlt.Length > MaxAltLength)
        {
            throw new ArgumentException($"alt maksimal {MaxAltLength} karakter (diterima {finalAlt.Length}).", nameof(alt));
        }

        var finalCaption = Blank(caption);
        if (finalCaption is { Length: > MaxCaptionLength })
        {
            throw new ArgumentException($"caption maksimal {MaxCaptionLength} karakter (diterima {finalCaption.Length}).", nameof(caption));
        }

        var finalLicense = license?.Trim() ?? string.Empty;
        if (finalLicense.Length == 0)
        {
            throw new ArgumentException($"Lisensi wajib. Karya yang digambar sendiri ditulis '{OwnWork}'.", nameof(license));
        }

        // Karya sendiri ditulis baku supaya CHECK di basis data bisa membandingkannya persis.
        if (string.Equals(finalLicense, OwnWork, StringComparison.OrdinalIgnoreCase))
        {
            finalLicense = OwnWork;
        }

        if (finalLicense.Length > MaxLicenseLength)
        {
            throw new ArgumentException($"license maksimal {MaxLicenseLength} karakter (diterima {finalLicense.Length}).", nameof(license));
        }

        var finalSourceName = Blank(sourceName);
        var finalSourceUrl = Blank(sourceUrl);

        if (finalSourceName is { Length: > MaxSourceNameLength })
        {
            throw new ArgumentException($"sourceName maksimal {MaxSourceNameLength} karakter (diterima {finalSourceName.Length}).", nameof(sourceName));
        }

        if (finalSourceUrl is not null
            && (finalSourceUrl.Length > MaxSourceUrlLength
                || !Uri.TryCreate(finalSourceUrl, UriKind.Absolute, out var parsed)
                || (parsed.Scheme != Uri.UriSchemeHttp && parsed.Scheme != Uri.UriSchemeHttps)))
        {
            throw new ArgumentException($"sourceUrl harus URL http/https absolut (maksimal {MaxSourceUrlLength} karakter), bukan '{sourceUrl}'.", nameof(sourceUrl));
        }

        if (finalLicense != OwnWork && (finalSourceName is null || finalSourceUrl is null))
        {
            throw new ArgumentException(
                $"Selain karya sendiri ('{OwnWork}'), sumber wajib: sourceName DAN sourceUrl. Media tanpa asal-usul yang bisa diperiksa tak boleh tayang.", nameof(sourceName));
        }

        return (finalKey, kind, finalUrl, finalVideoId, finalAlt, finalCaption, finalSourceName, finalSourceUrl, finalLicense);
    }

    private static string? Blank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    [GeneratedRegex("^[a-z0-9]+(-[a-z0-9]+)*$")]
    private static partial Regex KeyPattern();

    [GeneratedRegex(@"^/media/[A-Za-z0-9._/-]+\.(svg|png|jpe?g|webp|avif)$", RegexOptions.IgnoreCase)]
    private static partial Regex OwnFilePattern();

    [GeneratedRegex("^[A-Za-z0-9_-]{11}$")]
    private static partial Regex VideoIdPattern();
}
