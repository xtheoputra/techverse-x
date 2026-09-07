namespace TechVerseX.TechnologyService.Domain;

/// <summary>
/// Akar agregat bidang Technology — "jantung TechVerse X" (KERANGKA.md 4.6).
/// </summary>
/// <remarks>
/// Satu topik di bawah satu <see cref="Field"/>. Isi halamannya — roadmap, tools,
/// mini project, resources (ADR-012) — belum dimodelkan di sini; itu irisan
/// berikutnya. Yang sudah ada: tempatnya di taksonomi, dan seberapa dipercaya
/// isinya.
/// </remarks>
public sealed class Technology
{
    /// <summary>
    /// Panjang maksimum <see cref="Slug"/>, sama dengan lebar kolom
    /// <c>technologies.slug</c>.
    /// </summary>
    /// <remarks>
    /// Angkanya hidup di sini supaya penjaga di lapisan masukan dan lebar kolom
    /// tidak bisa berjalan sendiri-sendiri. Sebelum ada konstanta ini, validator
    /// membatasi slug yang DIKIRIM tapi tidak slug yang DITURUNKAN dari nama —
    /// nama 200 karakter lolos, lalu ditolak basis data sebagai galat server.
    /// </remarks>
    public const int MaxSlugLength = 160;

    private readonly List<DomainEvent> _events = [];
    private readonly List<TechnologyRelationship> _relationships = [];

    private Technology()
    {
        // Dipakai EF Core.
        Slug = string.Empty;
        Name = string.Empty;
        Summary = string.Empty;
    }

    private Technology(Guid id, string slug, string name, string summary, Guid fieldId)
    {
        Id = id;
        Slug = slug;
        Name = name;
        Summary = summary;
        FieldId = fieldId;
        Status = TechnologyStatus.Draft;
        Maturity = ContentMaturity.Curated;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public Guid Id { get; private set; }

    /// <summary>Kunci yang dipakai di URL — lihat KERANGKA.md 4.9 <c>/api/v1/technologies/{slug}</c>.</summary>
    public string Slug { get; private set; }

    public string Name { get; private set; }

    public string Summary { get; private set; }

    /// <summary>
    /// Bidang tempat topik ini duduk. Dulu ini teks bebas bernama <c>Category</c>;
    /// ia naik jadi kunci asing setelah taksonomi diputuskan (ADR-010, ADR-015).
    /// </summary>
    public Guid FieldId { get; private set; }

    /// <summary>Boleh tayang? Lihat <see cref="TechnologyStatus"/>.</summary>
    public TechnologyStatus Status { get; private set; }

    /// <summary>
    /// Seberapa dipercaya isinya? Sumbu yang BERBEDA dari <see cref="Status"/> —
    /// lihat <see cref="ContentMaturity"/> dan ADR-015 bagian 1.
    /// </summary>
    public ContentMaturity Maturity { get; private set; }

    /// <summary>Kapan manusia terakhir memeriksanya. Null berarti belum pernah.</summary>
    public DateTimeOffset? ReviewedAt { get; private set; }

    /// <summary>Siapa yang memeriksanya. Null berarti belum pernah.</summary>
    public string? ReviewedBy { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public IReadOnlyList<TechnologyRelationship> Relationships => _relationships;

    public IReadOnlyList<DomainEvent> Events => _events;

    public static Technology Create(string name, string summary, Guid fieldId, string? slug = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (fieldId == Guid.Empty)
        {
            throw new ArgumentException("Setiap teknologi harus duduk di satu bidang.", nameof(fieldId));
        }

        var finalSlug = string.IsNullOrWhiteSpace(slug) ? Slugs.From(name) : Slugs.From(slug);

        // Guid v7 berurut menurut waktu — indeks primer tidak terfragmentasi
        // seperti kalau memakai Guid acak.
        var technology = new Technology(Guid.CreateVersion7(), finalSlug, name.Trim(), summary?.Trim() ?? string.Empty, fieldId);
        technology._events.Add(new TechnologyCreated(technology.Id, technology.Slug, technology.Name));
        return technology;
    }

    /// <summary>
    /// Mengubah isi. <b>Sengaja menurunkan kembali kematangan yang sudah
    /// <see cref="ContentMaturity.HumanReviewed"/></b>: pemeriksaan manusia berlaku
    /// atas teks yang diperiksa, bukan atas nama halamannya. Kalau teksnya berubah,
    /// pemeriksaan itu tidak lagi menjangkau isi yang sekarang.
    /// </summary>
    public void Update(string name, string summary, Guid fieldId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (fieldId == Guid.Empty)
        {
            throw new ArgumentException("Setiap teknologi harus duduk di satu bidang.", nameof(fieldId));
        }

        Name = name.Trim();
        Summary = summary?.Trim() ?? string.Empty;
        FieldId = fieldId;
        UpdatedAt = DateTimeOffset.UtcNow;

        if (Maturity == ContentMaturity.HumanReviewed)
        {
            Maturity = ContentMaturity.MachineDrafted;
            ReviewedAt = null;
            ReviewedBy = null;
            _events.Add(new TechnologyReviewExpired(Id, Slug));
        }

        _events.Add(new TechnologyUpdated(Id, Slug));
    }

    /// <summary>Menaikkan isi ke tingkat draf mesin — kelima bagian terisi, belum diperiksa.</summary>
    public void MarkDrafted()
    {
        if (Maturity == ContentMaturity.HumanReviewed)
        {
            throw new InvalidOperationException(
                $"Teknologi '{Slug}' sudah diperiksa manusia. Turunkan lewat Update(), bukan dengan menandainya draf.");
        }

        if (Maturity == ContentMaturity.MachineDrafted)
        {
            return;
        }

        Maturity = ContentMaturity.MachineDrafted;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Menaikkan isi ke <see cref="ContentMaturity.HumanReviewed"/>. <b>Satu-satunya
    /// jalan ke tingkat itu</b>, dan ia menuntut nama pemeriksanya.
    /// </summary>
    /// <remarks>
    /// Aturan ADR-012 ditegakkan di sini, bukan di komentar: tidak ada cara menandai
    /// halaman "sudah diperiksa manusia" tanpa menyebut manusianya.
    /// </remarks>
    public void MarkReviewed(string reviewer)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reviewer);

        if (Maturity == ContentMaturity.Curated)
        {
            throw new InvalidOperationException(
                $"Teknologi '{Slug}' masih berupa kurasi tautan. Isinya harus lengkap dulu (MarkDrafted) sebelum ada yang bisa diperiksa.");
        }

        Maturity = ContentMaturity.HumanReviewed;
        ReviewedAt = DateTimeOffset.UtcNow;
        ReviewedBy = reviewer.Trim();
        UpdatedAt = ReviewedAt.Value;
        _events.Add(new TechnologyReviewed(Id, Slug, ReviewedBy));
    }

    /// <summary>
    /// Menerbitkan entri. Sengaja MENOLAK entri yang baru ditemukan agen dan
    /// belum diverifikasi — lihat pipeline di KERANGKA.md 4.6, yang menaruh
    /// Verification sebelum Publish justru supaya halusinasi tidak ikut tayang.
    /// </summary>
    /// <remarks>
    /// Perhatikan yang TIDAK diperiksa di sini: <see cref="Maturity"/>. Halaman
    /// kurasi maupun draf boleh terbit — itu memang keadaan normal menurut ADR-012.
    /// Yang wajib menyertainya adalah label, dan label itu dijamin kontrak API yang
    /// selalu membawa <see cref="Maturity"/>, bukan oleh larangan di sini.
    /// </remarks>
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

    /// <summary>Mengubah "AI Agents" menjadi "ai-agents". Sekarang meneruskan ke <see cref="Slugs"/>.</summary>
    public static string Slugify(string value) => Slugs.From(value);
}
