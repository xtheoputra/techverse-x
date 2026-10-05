namespace TechVerseX.TechnologyService.Domain;

/// <summary>
/// Akar agregat bidang Technology — "jantung TechVerse X" (KERANGKA.md 4.6).
/// </summary>
/// <remarks>
/// Satu topik di bawah satu <see cref="Field"/>, berikut <b>kelima bagian
/// template ADR-012</b>: Overview (<see cref="Summary"/>), Learning Roadmap
/// (<see cref="Roadmap"/>), Tools (<see cref="Tools"/>), Mini Project
/// (<see cref="Projects"/>), dan Resources (<see cref="Resources"/>).
/// <para>
/// Keempat bagian terakhir mendarat belakangan, dan kedatangannya mengubah satu
/// hal yang lebih penting daripada sekadar menambah tabel: <see cref="MarkDrafted"/>
/// selama ini <em>mengaku</em> berarti "kelima bagian terisi" padahal tidak ada
/// satu pun yang bisa diperiksanya. Sekarang ada, dan ia memeriksanya — lihat
/// <see cref="MissingSections"/>.
/// </para>
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

    // Empat bagian template yang dimiliki agregat ini. Perhatikan yang KETIGA:
    // ia menyimpan TAUTAN ke alat, bukan alatnya — Tool agregat sendiri, sebab
    // satu alat dipakai lintas topik (ADR-015).
    private readonly List<RoadmapStep> _roadmap = [];
    private readonly List<TechnologyTool> _tools = [];
    private readonly List<Project> _projects = [];
    private readonly List<Resource> _resources = [];

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

    /// <summary>
    /// Sisi KELUAR topik ini: topik-topik yang ia butuhkan (ADR-023). Ditulis hanya
    /// lewat <see cref="RequireTopic"/>.
    /// </summary>
    /// <remarks>
    /// Sisi MASUK ("dibutuhkan oleh") sengaja tidak ada di sini: ia milik agregat
    /// topik lain, dan diturunkan saat dibaca, bukan disimpan dua kali.
    /// </remarks>
    public IReadOnlyList<TechnologyRelationship> Relationships => _relationships;

    /// <summary>Bagian 2 template, berurut. Langkah <c>0</c> adalah prasyarat (ADR-012).</summary>
    public IReadOnlyList<RoadmapStep> Roadmap => _roadmap;

    /// <summary>Bagian 3 template — <b>tautan</b> ke alat, bukan salinannya.</summary>
    public IReadOnlyList<TechnologyTool> Tools => _tools;

    /// <summary>Bagian 4 template.</summary>
    public IReadOnlyList<Project> Projects => _projects;

    /// <summary>Bagian 5 template.</summary>
    public IReadOnlyList<Resource> Resources => _resources;

    public IReadOnlyList<DomainEvent> Events => _events;

    /// <summary>
    /// Bagian template ADR-012 yang masih kosong. <b>Kosong berarti kelimanya
    /// terisi</b>, dan hanya kalau kosong halaman ini boleh naik ke <c>draf</c>.
    /// </summary>
    /// <remarks>
    /// Ia mengembalikan DAFTAR, bukan sekadar <c>bool</c>, karena pesan
    /// "belum lengkap" yang tidak menyebut apanya memaksa penulis menebak. Nama
    /// bagiannya memakai istilah template supaya jawabannya bisa langsung
    /// dicocokkan ke ADR-012 tanpa penerjemahan.
    /// </remarks>
    public IReadOnlyList<string> MissingSections
    {
        get
        {
            var missing = new List<string>();

            if (string.IsNullOrWhiteSpace(Summary))
            {
                missing.Add("Overview");
            }

            // Roadmap yang cuma berisi prasyarat belum mengajari apa pun — ia
            // baru menyebut titik berangkatnya. Karena itu syaratnya BUKAN
            // "ada isinya", melainkan ada langkah SESUDAH langkah 0.
            if (_roadmap.Count < 2)
            {
                missing.Add("Learning Roadmap");
            }

            if (_tools.Count == 0)
            {
                missing.Add("Tools");
            }

            if (_projects.Count == 0)
            {
                missing.Add("Mini Project");
            }

            if (_resources.Count == 0)
            {
                missing.Add("Resources");
            }

            return missing;
        }
    }

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

    /// <summary>
    /// Menetapkan <b>langkah 0 — prasyarat</b> (ADR-012). Memanggilnya lagi
    /// mengganti prasyarat yang ada, bukan menambah langkah kedua bernomor nol.
    /// </summary>
    /// <remarks>
    /// Ia harus dipanggil sebelum <see cref="AddRoadmapStep"/>. Bukan demi
    /// kerapian: kalau langkah biasa boleh lebih dulu, langkah pertama yang
    /// masuk akan mendapat nomor 0 dan diam-diam menjadi "prasyarat" tanpa ada
    /// yang bermaksud begitu.
    /// </remarks>
    public void SetPrerequisite(string title, string description)
    {
        var step = RoadmapStep.Create(Id, RoadmapStep.PrerequisiteOrder, title, description);

        var existing = _roadmap.FindIndex(s => s.Order == RoadmapStep.PrerequisiteOrder);
        if (existing >= 0)
        {
            _roadmap[existing] = step;
        }
        else
        {
            _roadmap.Insert(0, step);
        }

        Touch();
    }

    /// <summary>
    /// Menambah satu langkah roadmap di ujung. <b>Nomornya ditentukan di sini,
    /// bukan dikirim pemanggil</b> — lihat catatan di <see cref="RoadmapStep"/>.
    /// </summary>
    public void AddRoadmapStep(string title, string description)
    {
        if (_roadmap.Count == 0)
        {
            throw new InvalidOperationException(
                $"Roadmap '{Slug}' belum punya langkah 0. Panggil SetPrerequisite() dulu — ADR-012 menjadikan prasyarat langkah pertama, bukan bagian terpisah.");
        }

        _roadmap.Add(RoadmapStep.Create(Id, _roadmap.Count, title, description));
        Touch();
    }

    /// <summary>
    /// Menautkan sebuah <see cref="Tool"/> ke topik ini. Menautkan alat yang sama
    /// dua kali tidak menambah baris kedua; catatannya yang diperbarui.
    /// </summary>
    public void AttachTool(Guid toolId, string? note = null)
    {
        var link = TechnologyTool.Create(Id, toolId, note);

        var existing = _tools.FindIndex(t => t.ToolId == toolId);
        if (existing >= 0)
        {
            _tools[existing] = link;
        }
        else
        {
            _tools.Add(link);
        }

        Touch();
    }

    /// <summary>Menambah Mini Project — bagian 4 template.</summary>
    public void AddProject(string title, string brief)
    {
        _projects.Add(Project.Create(Id, title, brief));
        Touch();
    }

    /// <summary>Menambah sumber belajar — bagian 5 template.</summary>
    public void AddResource(ResourceType type, string title, string url)
    {
        _resources.Add(Resource.Create(Id, type, title, url));
        Touch();
    }

    /// <summary>
    /// Mencatat bahwa topik ini <b>membutuhkan</b> topik lain: pelajari
    /// <paramref name="topicId"/> lebih dulu (ADR-023). Mengulang sisi yang sama
    /// tidak menambah apa-apa.
    /// </summary>
    /// <param name="topicId">Topik yang dibutuhkan. Harus topik lain, bukan topik ini.</param>
    /// <param name="topicRequires">
    /// Segala yang <paramref name="topicId"/> butuhkan lewat sisi
    /// <see cref="RelationshipKind.Requires"/> — <b>penutupan transitifnya</b>, bukan
    /// tujuan langsungnya. <b>Wajib, tanpa nilai bawaan</b>: pemanggil yang bisa
    /// melupakannya adalah pemanggil yang bisa melewati aturan siklus di bawah, idiom
    /// yang sama dengan sakelar tanpa bawaan di ADR-020.
    /// <para>
    /// ⚠️ Agregat ini <b>tidak bisa tahu</b> apakah sebuah Id di dalamnya tujuan
    /// langsung atau ujung rantai — yang diterimanya himpunan rata. Itu yang membentuk
    /// kata-kata galatnya, dan alasan ia tidak menyebut "dua topik".
    /// </para>
    /// </param>
    /// <remarks>
    /// 🔑 <b>Bukan bagian template ADR-012.</b> Relasi tidak pernah dihitung
    /// <see cref="MissingSections"/>: halaman tanpa satu sisi pun tetap lengkap.
    /// <para>
    /// 🔴 <b>Prasyarat tidak boleh berputar.</b> Kalau topik tujuan sudah membutuhkan
    /// topik ini — langsung, atau lewat rantai sepanjang apa pun — sisi baru itu
    /// menutup lingkaran, jadi ia ditolak. Sampai 2026-09-24 hanya lingkaran berdua
    /// yang tertangkap, sebab yang diserahkan pemanggil cuma tujuan langsung tujuan;
    /// A→B→C→A lolos. Yang berubah memang hanya isi <paramref name="topicRequires"/>,
    /// seperti yang diramal ADR-023 — <b>tapi kalimat galatnya ikut berubah</b>: "dua
    /// topik tidak boleh saling mensyaratkan" adalah pernyataan yang salah ketika yang
    /// ditutup lingkaran bertiga.
    /// </para>
    /// <para>
    /// ⚠️ Seperti <see cref="Touch"/>, ia <b>tidak</b> menurunkan
    /// <see cref="ContentMaturity.HumanReviewed"/>: menautkan halaman yang sudah
    /// diperiksa ke topik lain bukan pembatalan pemeriksaan teksnya.
    /// </para>
    /// <para>
    /// Satu-satunya pemanggil produksi: <c>RequireTopicHandler</c>, yang di produksi
    /// hanya terjangkau lewat jalan ADR-021.
    /// </para>
    /// </remarks>
    public void RequireTopic(Guid topicId, IReadOnlyCollection<Guid> topicRequires)
    {
        ArgumentNullException.ThrowIfNull(topicRequires);

        // Diperiksa DI SINI, bukan diserahkan ke TechnologyRelationship.Create:
        // pesan dan nama medannya harus berbicara bahasa pemanggil (topicSlug di
        // muatan permintaan), bukan nama parameter internal. Penerjemah galat
        // mengubah ParamName langsung jadi nama medan ValidationProblem.
        //
        // ⚠️ CA2208 dimatikan untuk SATU baris ini saja, dengan sengaja. Aturan itu
        // benar untuk umumnya: ParamName seharusnya nama parameter metode. Di
        // agregat ini keduanya kebetulan selalu sama (nameof(url), nameof(name))
        // karena medan muatannya bernama sama dengan parameternya — kecuali di
        // sini, tempat yang dikirim pemanggil adalah SLUG sementara domain hanya
        // mengenal Id. "topicId" di ValidationProblem akan menunjuk medan yang
        // tidak pernah ada di permintaan mana pun.
        if (topicId == Id)
        {
#pragma warning disable CA2208
            throw new ArgumentException($"Topik '{Slug}' tidak bisa mensyaratkan dirinya sendiri.", "topicSlug");
#pragma warning restore CA2208
        }

        // Idempoten, dan sengaja SEBELUM pemeriksaan siklus: mengulang permintaan
        // yang sudah tercatat tidak boleh berubah jadi galat.
        //
        // ⚠️ Versi pertama komentar ini mengklaim urutan itu jadi LEBIH penting
        // sesudah penutupan transitif dipakai, sebab "sisi A→B yang sudah tersimpan
        // menaruh A di dalam penutupan B". Itu KELIRU: penutupan B berisi yang
        // DIBUTUHKAN B, dan A tidak ada di sana kecuali lingkarannya memang sudah
        // tertutup. Yang benar lebih sempit, dan hanya mengenai baris yang tersimpan
        // SEBELUM aturan ini ada: kalau B→C→A sudah ada, mengulang A→B kini melihat A
        // di penutupan B (dulu tidak, sebab tujuan langsung B cuma C) - jadi jalan
        // keluar di atas inilah yang menjaga permintaan ulang tetap 200 di data lama
        // yang sudah berputar.
        if (_relationships.Any(r => r.Kind == RelationshipKind.Requires && r.ToTechnologyId == topicId))
        {
            return;
        }

        if (topicRequires.Contains(Id))
        {
            throw new InvalidOperationException(
                $"Topik yang diminta sudah mensyaratkan '{Slug}', langsung atau lewat rantai prasyarat. Prasyarat tidak boleh berputar (ADR-023).");
        }

        _relationships.Add(TechnologyRelationship.Create(Id, topicId, RelationshipKind.Requires));
        Touch();
    }

    /// <summary>
    /// Menaikkan isi ke tingkat draf mesin — <b>kelima bagian terisi</b>, belum
    /// diperiksa.
    /// </summary>
    /// <remarks>
    /// 🔑 Kalimat "kelima bagian terisi" dulu hanya ada di ringkasan ini, dan
    /// metodenya meluluskan halaman kosong. Sejak keempat entitas isi mendarat,
    /// ia <b>diperiksa</b> — dan pesan gagalnya menyebut bagian mana yang kurang,
    /// supaya penulisnya tidak perlu menebak.
    /// </remarks>
    public void MarkDrafted()
    {
        if (Maturity == ContentMaturity.HumanReviewed)
        {
            throw new InvalidOperationException(
                $"Teknologi '{Slug}' sudah diperiksa manusia. Turunkan lewat Update(), bukan dengan menandainya draf.");
        }

        var missing = MissingSections;
        if (missing.Count > 0)
        {
            throw new InvalidOperationException(
                $"Teknologi '{Slug}' belum bisa jadi draf — bagian template yang masih kosong: {string.Join(", ", missing)}.");
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

    /// <summary>
    /// Mencatat bahwa isi halaman berubah.
    /// </summary>
    /// <remarks>
    /// ⚠️ Sengaja <b>tidak</b> menurunkan <see cref="ContentMaturity.HumanReviewed"/>
    /// seperti <see cref="Update"/>. Alasannya bukan kelalaian: menambah satu
    /// sumber belajar ke halaman yang sudah diperiksa manusia bukan pembatalan
    /// pemeriksaan itu, dan kalau ia dianggap begitu, memperbaiki satu tautan
    /// mati akan membuang seluruh nilai kerja pemeriksanya.
    /// <para>
    /// Ini keputusan yang layak dibantah kalau ternyata salah — batasnya tipis,
    /// dan uji <c>MenambahBagian_TIDAKMenggugurkanPemeriksaanManusia</c> ada
    /// supaya perubahan pendapat soal ini harus disengaja.
    /// </para>
    /// </remarks>
    private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
}
