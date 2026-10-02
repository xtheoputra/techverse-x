using TechVerseX.TechnologyService.Domain;

namespace TechVerseX.TechnologyService.Tests.Domain;

/// <summary>
/// Penjaga keempat bagian isi template ADR-012: Learning Roadmap, Tools,
/// Mini Project, Resources.
/// </summary>
/// <remarks>
/// Yang diuji di sini bukan "bisa menyimpan data", melainkan <b>bentuk yang
/// tidak boleh bisa dilanggar</b> — alasan ADR-015 memilih tabel bertipe
/// daripada blok generik. Tiga bentuk itu:
/// <list type="number">
///   <item>Langkah 0 roadmap adalah prasyarat, dan nomor langkah tidak berlubang.</item>
///   <item>Alat ditautkan, tidak disalin.</item>
///   <item>"Kelima bagian terisi" adalah syarat yang diperiksa, bukan kalimat.</item>
/// </list>
/// </remarks>
public sealed class BagianIsiHalamanTests
{
    private static readonly Guid AnyField = FieldCatalog.All[0].Id;

    private static Technology Kosong() =>
        Technology.Create("AI Agents", "Dari AI yang menjawab ke AI yang mengerjakan.", AnyField);

    // ---- "Kelima bagian terisi" berhenti jadi kalimat ----------------------

    [Fact]
    public void MarkDrafted_MenolakHalamanYangBagiannyaBelumTerisi()
    {
        // Inilah aturan yang selama ini hanya ditulis di ringkasan MarkDrafted
        // dan tidak diperiksa apa pun. Enam uji lama memerah saat ia dipasang.
        var technology = Kosong();

        var error = Assert.Throws<InvalidOperationException>(technology.MarkDrafted);

        Assert.Equal(ContentMaturity.Curated, technology.Maturity);
        Assert.Contains("Learning Roadmap", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void MissingSections_MenyebutBagianMANAYangKurang_bukanSekadarGagal()
    {
        // Pesan "belum lengkap" yang tidak menyebut apanya memaksa penulis menebak.
        var technology = Kosong();

        Assert.Equal(
            ["Learning Roadmap", "Tools", "Mini Project", "Resources"],
            technology.MissingSections);
    }

    [Fact]
    public void MissingSections_KosongSetelahKelimanyaTerisi()
    {
        var technology = Kosong().IsiKelimaBagian();

        Assert.Empty(technology.MissingSections);
        technology.MarkDrafted();
        Assert.Equal(ContentMaturity.MachineDrafted, technology.Maturity);
    }

    [Fact]
    public void RoadmapYangHanyaBerisiPrasyarat_BelumDianggapLengkap()
    {
        // Roadmap yang cuma menyebut titik berangkat belum mengajari apa pun.
        var technology = Kosong();
        technology.SetPrerequisite("Prasyarat", "Python dasar.");
        technology.AttachTool(TemplateLengkap.AlatContoh);
        technology.AddProject("Proyek", "Sesuatu yang jalan.");
        technology.AddResource(ResourceType.OfficialDocs, "Docs", "https://example.com");

        Assert.Contains("Learning Roadmap", technology.MissingSections);

        technology.AddRoadmapStep("Langkah 1", "Menjalankan contoh terkecil.");

        Assert.Empty(technology.MissingSections);
    }

    // ---- Bagian 2: langkah 0 adalah prasyarat ------------------------------

    [Fact]
    public void SetPrerequisite_MenjadiLangkahNol()
    {
        var technology = Kosong();

        technology.SetPrerequisite("Prasyarat", "Python dasar.");

        var step = Assert.Single(technology.Roadmap);
        Assert.Equal(RoadmapStep.PrerequisiteOrder, step.Order);
        Assert.True(step.IsPrerequisite);
    }

    [Fact]
    public void AddRoadmapStep_MenolakDipanggilSebelumAdaPrasyarat()
    {
        // Kalau langkah biasa boleh lebih dulu, langkah pertama yang masuk akan
        // mendapat nomor 0 dan diam-diam menjadi "prasyarat" tanpa ada yang
        // bermaksud begitu.
        var technology = Kosong();

        var error = Assert.Throws<InvalidOperationException>(
            () => technology.AddRoadmapStep("Langkah 1", "Isi."));

        Assert.Contains("SetPrerequisite", error.Message, StringComparison.Ordinal);
        Assert.Empty(technology.Roadmap);
    }

    [Fact]
    public void AddRoadmapStep_MemberiNomorBerurutTanpaLubang()
    {
        // Nomor TIDAK pernah dikirim pemanggil - itu yang membuat roadmap
        // berlubang (0, 1, 4) mustahil, bukan sekadar tidak dianjurkan.
        var technology = Kosong();
        technology.SetPrerequisite("Prasyarat", "Dasar.");

        technology.AddRoadmapStep("Satu", "a");
        technology.AddRoadmapStep("Dua", "b");
        technology.AddRoadmapStep("Tiga", "c");

        Assert.Equal([0, 1, 2, 3], technology.Roadmap.Select(s => s.Order));
        Assert.Single(technology.Roadmap, s => s.IsPrerequisite);
    }

    [Fact]
    public void SetPrerequisite_DipanggilDuaKaliMengganti_bukanMenambahLangkahNolKedua()
    {
        var technology = Kosong();
        technology.SetPrerequisite("Prasyarat lama", "a");
        technology.AddRoadmapStep("Satu", "b");

        technology.SetPrerequisite("Prasyarat baru", "c");

        Assert.Equal(2, technology.Roadmap.Count);
        Assert.Single(technology.Roadmap, s => s.Order == RoadmapStep.PrerequisiteOrder);
        Assert.Equal("Prasyarat baru", technology.Roadmap[0].Title);

        // Langkah sesudahnya tidak ikut bergeser nomornya.
        Assert.Equal(1, technology.Roadmap[1].Order);
    }

    // ---- Bagian 3: alat ditautkan, bukan disalin ---------------------------

    [Fact]
    public void Tools_MenyimpanTAUTAN_bukanSalinanAlat()
    {
        // Ditegakkan TIPE: kalau koleksi ini suatu saat berubah jadi
        // IReadOnlyList<Tool>, uji ini tidak akan bisa dikompilasi lagi - dan
        // itu memang maksudnya. ADR-015: menyalin alat per halaman menjamin data
        // yang saling bertentangan.
        var technology = Kosong().IsiKelimaBagian();

        IReadOnlyList<TechnologyTool> tautan = technology.Tools;

        Assert.Equal(TemplateLengkap.AlatContoh, Assert.Single(tautan).ToolId);
    }

    [Fact]
    public void AttachTool_AlatYangSamaTidakTercatatDuaKali()
    {
        var technology = Kosong();
        var alat = Guid.CreateVersion7();

        technology.AttachTool(alat, "catatan pertama");
        technology.AttachTool(alat, "catatan kedua");

        Assert.Equal("catatan kedua", Assert.Single(technology.Tools).Note);
    }

    [Fact]
    public void AttachTool_MenolakAlatKosong()
    {
        var technology = Kosong();

        Assert.Throws<ArgumentException>(() => technology.AttachTool(Guid.Empty));
    }

    // ---- Bagian 5: sumber yang tidak bisa dibuka bukan sumber --------------

    [Theory]
    [InlineData("/docs/panduan")]
    [InlineData("example.com/docs")]
    [InlineData("javascript:alert(1)")]
    [InlineData("file:///etc/passwd")]
    public void AddResource_MenolakUrlYangTidakBisaDibukaPembaca(string url)
    {
        var technology = Kosong();

        Assert.Throws<ArgumentException>(
            () => technology.AddResource(ResourceType.OfficialDocs, "Panduan", url));
    }

    [Fact]
    public void AddResource_MenerimaKeempatJenisnya()
    {
        // Keempatnya datang apa adanya dari ADR-012 bagian 1.
        var technology = Kosong();

        foreach (var jenis in Enum.GetValues<ResourceType>())
        {
            technology.AddResource(jenis, $"Sumber {jenis}", "https://example.com/x");
        }

        Assert.Equal(4, technology.Resources.Count);
        Assert.Equal(Enum.GetValues<ResourceType>(), technology.Resources.Select(r => r.Type));
    }

    // ---- Bagian 4 --------------------------------------------------------

    [Fact]
    public void AddProject_MenuntutPenjelasan_bukanCumaJudul()
    {
        // "Satu proyek bisa dikerjakan orang dari awal sampai selesai" adalah
        // bukti selesai Bulan 6 di RENCANA-V1. Proyek tanpa penjelasan itu judul.
        var technology = Kosong();

        Assert.Throws<ArgumentException>(() => technology.AddProject("Proyek", "   "));
    }

    // ---- Membuang bagian isi: idempoten, tidak menurunkan tingkat ---------

    [Fact]
    public void RemoveResource_membuang_lalu_idempoten()
    {
        var technology = Kosong();
        technology.AddResource(ResourceType.OfficialDocs, "Docs", "https://example.com");
        var id = technology.Resources[0].Id;

        technology.RemoveResource(id);
        Assert.Empty(technology.Resources);

        // Idempoten: membuang lagi, dan membuang Id yang tak pernah ada, tak melempar.
        technology.RemoveResource(id);
        technology.RemoveResource(Guid.NewGuid());
        Assert.Empty(technology.Resources);
    }

    [Fact]
    public void RemoveProject_membuang_lalu_idempoten()
    {
        var technology = Kosong();
        technology.AddProject("Proyek", "Sesuatu yang jalan.");
        var id = technology.Projects[0].Id;

        technology.RemoveProject(id);
        Assert.Empty(technology.Projects);

        technology.RemoveProject(id);
        technology.RemoveProject(Guid.NewGuid());
        Assert.Empty(technology.Projects);
    }

    [Fact]
    public void DetachTool_melepas_tautan_lalu_idempoten()
    {
        var technology = Kosong();
        var alat = Guid.CreateVersion7();
        technology.AttachTool(alat, "catatan");

        technology.DetachTool(alat);
        Assert.Empty(technology.Tools);

        technology.DetachTool(alat);
        technology.DetachTool(Guid.CreateVersion7());
        Assert.Empty(technology.Tools);
    }

    [Fact]
    public void MembuangBagian_TIDAKMenggugurkanPemeriksaanManusia()
    {
        // Cermin MenambahBagian_TIDAK...: ADR-012 menalar memperbaiki tautan mati
        // (buang lalu tambah) tidak boleh membuang nilai kerja pemeriksa, jadi buang
        // pun tidak menggugurkan. Ditambah satu sumber ekstra lalu dibuang supaya
        // halamannya tetap lengkap — yang diuji murni efek buang pada kematangan.
        var technology = Kosong().IsiKelimaBagian();
        technology.MarkDrafted();
        technology.MarkReviewed("pemilik");
        technology.AddResource(ResourceType.Video, "Ekstra", "https://example.com/v");
        var ekstra = technology.Resources[^1].Id;

        technology.RemoveResource(ekstra);

        Assert.Equal(ContentMaturity.HumanReviewed, technology.Maturity);
        Assert.Equal("pemilik", technology.ReviewedBy);
    }

    // ---- Batas yang sengaja dipilih, dan layak dibantah -------------------

    [Fact]
    public void MenambahBagian_TIDAKMenggugurkanPemeriksaanManusia()
    {
        // Berbeda dari Update(), yang MEMANG menggugurkan. Alasannya: menambah
        // satu sumber belajar bukan pembatalan pemeriksaan, dan kalau dianggap
        // begitu, memperbaiki satu tautan mati akan membuang seluruh nilai kerja
        // pemeriksanya.
        //
        // Batasnya tipis. Uji ini ada supaya perubahan pendapat soal ini harus
        // DISENGAJA, bukan terjadi diam-diam.
        var technology = Kosong().IsiKelimaBagian();
        technology.MarkDrafted();
        technology.MarkReviewed("pemilik");

        technology.AddResource(ResourceType.Video, "Kuliah tambahan", "https://example.com/v");

        Assert.Equal(ContentMaturity.HumanReviewed, technology.Maturity);
        Assert.Equal("pemilik", technology.ReviewedBy);
    }

    [Fact]
    public void Tool_MenolakNamaYangTidakMenyisakanSlug()
    {
        // Jalur yang sama dengan Technology: Slugs.From MELEMPAR, dan lapisan
        // masukan yang bertugas mengubahnya jadi 400 - bukan domainnya.
        Assert.ThrowsAny<ArgumentException>(() => Tool.Create("!!!", "Ringkasan."));
    }

    [Fact]
    public void Tool_MenurunkanSlugDariNamanya()
    {
        var tool = Tool.Create("LangGraph Studio", "Alat visual.", "https://example.com");

        Assert.Equal("langgraph-studio", tool.Slug);
        Assert.Equal("https://example.com", tool.Homepage);
    }
}
