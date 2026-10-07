using TechVerseX.TechnologyService.Domain;

namespace TechVerseX.TechnologyService.Tests.Domain;

/// <summary>
/// ADR-028 Tahap 2 / #79: jalan UBAH untuk isi yang sudah terpasang — nama dan
/// ringkasan topik, langkah roadmap menurut nomornya, dan katalog alat.
/// </summary>
/// <remarks>
/// 🔑 Aturannya satu, dan sudah diputuskan pemilik 2026-10-06 (#81): <b>mengganti teks
/// yang dibaca pemeriksa menggugurkan <c>tinjau</c>; mengulang teks yang sama tidak</b>.
/// Berkas ini hanya menerapkannya ke tiga pintu yang baru — plus satu pintu lama
/// (<see cref="Technology.Update"/>) yang ternyata belum mematuhinya.
/// </remarks>
public sealed class JalanUbahTests
{
    private static readonly Guid AnyField = FieldCatalog.All[0].Id;

    private static Technology Ditinjau()
    {
        var technology = Technology.Create("AI Agents", "Dari AI yang menjawab ke AI yang mengerjakan.", AnyField)
            .IsiKelimaBagian();
        technology.MarkDrafted();
        technology.MarkReviewed("pemilik");
        technology.ClearEvents();
        return technology;
    }

    private static void AssertTetapDitinjau(Technology technology)
    {
        Assert.Equal(ContentMaturity.HumanReviewed, technology.Maturity);
        Assert.Equal("pemilik", technology.ReviewedBy);
        Assert.NotNull(technology.ReviewedAt);
        Assert.DoesNotContain(technology.Events, e => e.EventType == nameof(TechnologyReviewExpired));
    }

    // ---- Technology.Update: PUT yang diulang idempoten ----------------------

    [Fact]
    public void Update_DenganIsiYangSama_TidakMenggugurkanPemeriksaan()
    {
        var technology = Ditinjau();

        technology.Update("AI Agents", "Dari AI yang menjawab ke AI yang mengerjakan.", AnyField);

        AssertTetapDitinjau(technology);
        Assert.Empty(technology.Events);
    }

    [Fact]
    public void Update_DenganIsiYangSamaSesudahDipangkas_TidakMenggugurkanPemeriksaan()
    {
        var technology = Ditinjau();

        technology.Update("  AI Agents ", "\nDari AI yang menjawab ke AI yang mengerjakan.  ", AnyField);

        AssertTetapDitinjau(technology);
    }

    [Fact]
    public void Update_DenganRingkasanBerbeda_TetapMenggugurkanPemeriksaan()
    {
        // Kendali arah berlawanan: tanpa ini, "tak pernah menggugurkan" juga hijau.
        var technology = Ditinjau();

        technology.Update("AI Agents", "Ringkasan yang lain sama sekali.", AnyField);

        Assert.Equal(ContentMaturity.MachineDrafted, technology.Maturity);
        Assert.Single(technology.Events, e => e.EventType == nameof(TechnologyReviewExpired));
        Assert.Single(technology.Events, e => e.EventType == nameof(TechnologyUpdated));
    }

    [Fact]
    public void Update_MenolakNamaDanRingkasanYangKepanjangan()
    {
        var technology = Ditinjau();

        var nama = Assert.Throws<ArgumentException>(
            () => technology.Update(new string('n', Technology.MaxNameLength + 1), "Ringkasan.", AnyField));
        Assert.Equal("name", nama.ParamName);

        var ringkasan = Assert.Throws<ArgumentException>(
            () => technology.Update("AI Agents", new string('r', Technology.MaxSummaryLength + 1), AnyField));
        Assert.Equal("summary", ringkasan.ParamName);

        // Yang ditolak tak mengubah apa pun.
        AssertTetapDitinjau(technology);
        Assert.Equal("AI Agents", technology.Name);
    }

    // ---- Technology.ReplaceRoadmapStep -------------------------------------

    [Fact]
    public void ReplaceRoadmapStep_MenggantiLangkahBiasaDiTempat_DanMenggugurkanPemeriksaan()
    {
        var technology = Ditinjau();
        var idSebelum = technology.Roadmap[1].Id;

        technology.ReplaceRoadmapStep(1, "Judul baru", "Uraian baru.");

        Assert.Equal(2, technology.Roadmap.Count);
        Assert.Equal("Judul baru", technology.Roadmap[1].Title);
        Assert.Equal("Uraian baru.", technology.Roadmap[1].Description);
        Assert.Equal(1, technology.Roadmap[1].Order);
        Assert.Equal(idSebelum, technology.Roadmap[1].Id);
        Assert.Equal(ContentMaturity.MachineDrafted, technology.Maturity);
        Assert.Null(technology.ReviewedBy);
        Assert.Single(technology.Events, e => e.EventType == nameof(TechnologyReviewExpired));
    }

    [Fact]
    public void ReplaceRoadmapStep_DenganTeksYangSama_TidakMengubahApaPun()
    {
        var technology = Ditinjau();
        var diperbarui = technology.UpdatedAt;

        technology.ReplaceRoadmapStep(1, "  Langkah pertama ", "Menjalankan contoh paling kecil.\n");

        AssertTetapDitinjau(technology);
        Assert.Equal(diperbarui, technology.UpdatedAt);
    }

    [Fact]
    public void ReplaceRoadmapStep_Nol_BerartiPrasyarat()
    {
        var technology = Ditinjau();

        technology.ReplaceRoadmapStep(0, "Prasyarat baru", "Python dasar saja cukup.");

        Assert.Equal(2, technology.Roadmap.Count);
        Assert.Equal("Prasyarat baru", technology.Roadmap[0].Title);
        Assert.True(technology.Roadmap[0].IsPrerequisite);
        Assert.Equal(ContentMaturity.MachineDrafted, technology.Maturity);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(7)]
    [InlineData(-1)]
    public void ReplaceRoadmapStep_MenolakNomorYangBelumAda_TanpaMembuatLubang(int nomor)
    {
        // Nomor adalah ALAMAT, bukan isian: menunjuk langkah yang belum ada tidak
        // membuatnya. Inilah yang menjaga "roadmap berurut tanpa lubang" tetap utuh.
        var technology = Ditinjau();

        var galat = Assert.Throws<InvalidOperationException>(
            () => technology.ReplaceRoadmapStep(nomor, "Judul", "Uraian"));

        Assert.Contains("0..1", galat.Message, StringComparison.Ordinal);
        Assert.Equal([0, 1], technology.Roadmap.Select(s => s.Order));
        AssertTetapDitinjau(technology);
    }

    [Fact]
    public void ReplaceRoadmapStep_PadaRoadmapKosong_MenolakDenganPesanYangJelas()
    {
        var technology = Technology.Create("Topik Kosong", "Ringkasan.", AnyField);

        var galat = Assert.Throws<InvalidOperationException>(
            () => technology.ReplaceRoadmapStep(1, "Judul", "Uraian"));

        Assert.Contains("belum punya langkah apa pun", galat.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ReplaceRoadmapStep_MenolakJudulKosongDanTeksKepanjangan_TanpaMengubahApaPun()
    {
        var technology = Ditinjau();

        Assert.Throws<ArgumentException>(() => technology.ReplaceRoadmapStep(1, "   ", "Uraian"));

        var judul = Assert.Throws<ArgumentException>(
            () => technology.ReplaceRoadmapStep(1, new string('j', RoadmapStep.MaxTitleLength + 1), "Uraian"));
        Assert.Equal("title", judul.ParamName);

        var uraian = Assert.Throws<ArgumentException>(
            () => technology.ReplaceRoadmapStep(1, "Judul", new string('u', RoadmapStep.MaxDescriptionLength + 1)));
        Assert.Equal("description", uraian.ParamName);

        Assert.Equal("Langkah pertama", technology.Roadmap[1].Title);
        AssertTetapDitinjau(technology);
    }

    [Fact]
    public void AddRoadmapStep_MenolakTeksKepanjangan()
    {
        // Penjaga lebar kini tinggal di RoadmapStep.Create, jadi jalan TAMBAH ikut
        // terjaga: sebelumnya hanya kolom basis data yang menolak, sebagai 500.
        var technology = Technology.Create("Topik Kosong", "Ringkasan.", AnyField);
        technology.SetPrerequisite("Prasyarat", "Uraian.");

        Assert.Throws<ArgumentException>(
            () => technology.AddRoadmapStep("Judul", new string('u', RoadmapStep.MaxDescriptionLength + 1)));
        Assert.Throws<ArgumentException>(
            () => technology.SetPrerequisite(new string('j', RoadmapStep.MaxTitleLength + 1), "Uraian."));

        Assert.Single(technology.Roadmap);
    }

    // ---- Tool.Update dan Technology.LinkedToolTextChanged ------------------

    [Fact]
    public void ToolUpdate_MelaporkanPerubahanTeksTetapiBukanPerubahanBeranda()
    {
        var alat = Tool.Create("LangGraph", "Kerangka alur agen.", "https://langchain-ai.github.io/langgraph/");

        Assert.False(alat.Update("LangGraph", "  Kerangka alur agen.\n", "https://example.com/langgraph"));
        Assert.Equal("https://example.com/langgraph", alat.Homepage);

        Assert.True(alat.Update("LangGraph", "Kerangka alur agen berstatus.", "https://example.com/langgraph"));
        Assert.Equal("Kerangka alur agen berstatus.", alat.Summary);

        Assert.True(alat.Update("LangGraph 1.0", "Kerangka alur agen berstatus.", null));
        Assert.Null(alat.Homepage);
        Assert.Equal("langgraph", alat.Slug);
    }

    [Fact]
    public void ToolUpdate_MenolakNamaKosongDanTeksKepanjangan_TanpaMengubahApaPun()
    {
        var alat = Tool.Create("LangGraph", "Kerangka alur agen.", "https://example.com");

        Assert.Throws<ArgumentException>(() => alat.Update("  ", "Ringkasan", null));
        Assert.Equal("name", Assert.Throws<ArgumentException>(() => alat.Update(new string('n', Tool.MaxNameLength + 1), "Ringkasan", null)).ParamName);
        Assert.Equal("summary", Assert.Throws<ArgumentException>(() => alat.Update("Nama", new string('r', Tool.MaxSummaryLength + 1), null)).ParamName);
        Assert.Equal("homepage", Assert.Throws<ArgumentException>(() => alat.Update("Nama", "Ringkasan", new string('h', Tool.MaxHomepageLength + 1))).ParamName);

        Assert.Equal("LangGraph", alat.Name);
        Assert.Equal("Kerangka alur agen.", alat.Summary);
        Assert.Equal("https://example.com", alat.Homepage);
    }

    [Fact]
    public void LinkedToolTextChanged_PadaAlatMilikTopik_MenggugurkanPemeriksaan()
    {
        var technology = Ditinjau();

        technology.LinkedToolTextChanged(TemplateLengkap.AlatContoh);

        Assert.Equal(ContentMaturity.MachineDrafted, technology.Maturity);
        Assert.Null(technology.ReviewedBy);
        Assert.Single(technology.Events, e => e.EventType == nameof(TechnologyReviewExpired));
    }

    [Fact]
    public void LinkedToolTextChanged_PadaAlatBukanMilikTopik_TidakMengubahApaPun()
    {
        var technology = Ditinjau();

        technology.LinkedToolTextChanged(Guid.CreateVersion7());

        AssertTetapDitinjau(technology);
    }
}
