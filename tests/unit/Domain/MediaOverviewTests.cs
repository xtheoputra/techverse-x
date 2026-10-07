using TechVerseX.TechnologyService.Domain;

namespace TechVerseX.TechnologyService.Tests.Domain;

/// <summary>
/// ADR-028 Tahap 3b: bagian <c>overview</c> berformat dan media (gambar, diagram, video)
/// sebuah topik — aturan domainnya.
/// </summary>
/// <remarks>
/// 🔑 Dua janji yang dijaga bersama kendali arah berlawanannya: (1) <b>media tanpa asal-usul
/// tak bisa dibuat</b> — teks alternatif wajib, lisensi wajib, dan sumber wajib kecuali karya
/// sendiri; (2) <b>mengganti</b> isi menggugurkan <c>tinjau</c>, sedangkan menambah, membuang,
/// dan mengulang yang sama tidak (aturan ADR-012 yang sama dengan semua teks lain).
/// </remarks>
public sealed class MediaOverviewTests
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

    private static void Gambar(Technology t, string key = "arsitektur", string alt = "Diagram arsitektur", string url = "/media/mcp/arsitektur.svg")
        => t.UpsertMedia(key, MediaKind.Image, url, null, alt, "Keterangan.", null, null, TechnologyMedia.OwnWork);

    private static void AssertTetapDitinjau(Technology t)
    {
        Assert.Equal(ContentMaturity.HumanReviewed, t.Maturity);
        Assert.Equal("pemilik", t.ReviewedBy);
        Assert.DoesNotContain(t.Events, e => e.EventType == nameof(TechnologyReviewExpired));
    }

    private static void AssertDigugurkan(Technology t)
    {
        Assert.Equal(ContentMaturity.MachineDrafted, t.Maturity);
        Assert.Null(t.ReviewedBy);
        Assert.Single(t.Events, e => e.EventType == nameof(TechnologyReviewExpired));
    }

    // ---- overview -------------------------------------------------------------

    [Fact]
    public void SetOverview_MenyimpanTeksTerpangkas_DanKosongBerartiTanpaOverview()
    {
        var t = Technology.Create("Topik", "Ringkasan.", AnyField);

        t.SetOverview("  ## Mengapa\n\nIsi.  ");
        Assert.Equal("## Mengapa\n\nIsi.", t.Overview);

        t.SetOverview("   ");
        Assert.Null(t.Overview);

        t.SetOverview("Isi lagi.");
        t.SetOverview(null);
        Assert.Null(t.Overview);
    }

    [Fact]
    public void SetOverview_DenganIsiYangSama_TidakMengubahApaPun()
    {
        var t = Ditinjau();
        t.SetOverview("Isi yang sudah diperiksa.");
        t.MarkReviewed("pemilik");
        t.ClearEvents();
        var diperbarui = t.UpdatedAt;

        t.SetOverview("  Isi yang sudah diperiksa.\n");

        AssertTetapDitinjau(t);
        Assert.Equal(diperbarui, t.UpdatedAt);
    }

    [Fact]
    public void SetOverview_DenganIsiBerbeda_MenggugurkanPemeriksaan_TermasukMengosongkan()
    {
        var t = Ditinjau();
        t.SetOverview("Isi pertama.");
        t.MarkReviewed("pemilik");
        t.ClearEvents();

        t.SetOverview("Isi yang lain.");
        AssertDigugurkan(t);

        t.MarkReviewed("pemilik");
        t.ClearEvents();
        t.SetOverview(null);
        AssertDigugurkan(t);
    }

    [Fact]
    public void SetOverview_MenolakTeksKepanjangan_DenganNamaMedanOverview_TanpaMengubahApaPun()
    {
        var t = Ditinjau();

        var galat = Assert.Throws<ArgumentException>(() => t.SetOverview(new string('x', Technology.MaxOverviewLength + 1)));

        Assert.Equal("overview", galat.ParamName);
        Assert.Null(t.Overview);
        AssertTetapDitinjau(t);

        t.SetOverview(new string('x', Technology.MaxOverviewLength)); // tepat di batas: sah
        Assert.Equal(Technology.MaxOverviewLength, t.Overview!.Length);
    }

    [Fact]
    public void OverviewDanMedia_TidakPernahMasukMissingSections()
    {
        // Overview berformat memperdalam bagian Overview, ia bukan bagian keenam: halaman
        // tanpa keduanya tetap lengkap, dan halaman yang lengkap tetap lengkap tanpa keduanya.
        var t = Ditinjau();
        t.SetOverview("Isi.");
        Gambar(t);

        Assert.Empty(t.MissingSections);
    }

    // ---- media: bentuk yang sah ----------------------------------------------------

    [Fact]
    public void UpsertMedia_Gambar_MenyimpanBentukYangBenar()
    {
        var t = Ditinjau();

        t.UpsertMedia("arsitektur", MediaKind.Image, "/media/mcp/arsitektur.svg", null, "  Diagram arsitektur  ", " Tiga peran. ", null, null, "karya SENDIRI");

        var m = Assert.Single(t.Media);
        Assert.Equal("arsitektur", m.Key);
        Assert.Equal(MediaKind.Image, m.Kind);
        Assert.Equal("/media/mcp/arsitektur.svg", m.Url);
        Assert.Null(m.VideoId);
        Assert.Equal("Diagram arsitektur", m.Alt);
        Assert.Equal("Tiga peran.", m.Caption);
        Assert.Equal(TechnologyMedia.OwnWork, m.License); // dibakukan, supaya CHECK basis data bisa membandingkannya persis
    }

    [Fact]
    public void UpsertMedia_Video_ButuhIdSebelasKarakter_DanSumber()
    {
        var t = Ditinjau();

        t.UpsertMedia("demo", MediaKind.Video, null, "dQw4w9WgXcQ", "Demo resmi", null, "Kanal resmi", "https://www.youtube.com/@kanal", "Hak cipta pemilik kanal");

        var m = Assert.Single(t.Media);
        Assert.Equal(MediaKind.Video, m.Kind);
        Assert.Equal("dQw4w9WgXcQ", m.VideoId);
        Assert.Null(m.Url);
    }

    // ---- media: yang ditolak, dengan nama medan yang benar -----------------------------

    public static TheoryData<string, MediaKind, string?, string?, string, string?, string?, string?, string, string> Cacat => new()
    {
        // kunci
        { "key", MediaKind.Image, "/media/a/b.svg", null, "alt", null, null, null, TechnologyMedia.OwnWork, "" },
        { "key", MediaKind.Image, "/media/a/b.svg", null, "alt", null, null, null, TechnologyMedia.OwnWork, "Besar" },
        { "key", MediaKind.Image, "/media/a/b.svg", null, "alt", null, null, null, TechnologyMedia.OwnWork, "dua kata" },
        { "key", MediaKind.Image, "/media/a/b.svg", null, "alt", null, null, null, TechnologyMedia.OwnWork, "-diawali-hubung" },
        // gambar: url
        { "url", MediaKind.Image, null, null, "alt", null, null, null, TechnologyMedia.OwnWork, "k" },
        { "url", MediaKind.Image, "https://x.test/a.png", null, "alt", null, null, null, TechnologyMedia.OwnWork, "k" },
        { "url", MediaKind.Image, "/lain/a.svg", null, "alt", null, null, null, TechnologyMedia.OwnWork, "k" },
        { "url", MediaKind.Image, "/media/../rahasia.svg", null, "alt", null, null, null, TechnologyMedia.OwnWork, "k" },
        { "url", MediaKind.Image, "/media/a/b.exe", null, "alt", null, null, null, TechnologyMedia.OwnWork, "k" },
        { "url", MediaKind.Image, "//media/a.svg", null, "alt", null, null, null, TechnologyMedia.OwnWork, "k" },
        // gambar tak punya videoId; video tak punya url
        { "videoId", MediaKind.Image, "/media/a/b.svg", "dQw4w9WgXcQ", "alt", null, null, null, TechnologyMedia.OwnWork, "k" },
        { "url", MediaKind.Video, "/media/a/b.svg", "dQw4w9WgXcQ", "alt", null, "Kanal", "https://x.test", "Lisensi", "k" },
        // video: id
        { "videoId", MediaKind.Video, null, null, "alt", null, "Kanal", "https://x.test", "Lisensi", "k" },
        { "videoId", MediaKind.Video, null, "pendek", "alt", null, "Kanal", "https://x.test", "Lisensi", "k" },
        { "videoId", MediaKind.Video, null, "dQw4w9WgXc!", "alt", null, "Kanal", "https://x.test", "Lisensi", "k" },
        // teks alternatif dan lisensi wajib
        { "alt", MediaKind.Image, "/media/a/b.svg", null, "   ", null, null, null, TechnologyMedia.OwnWork, "k" },
        { "license", MediaKind.Image, "/media/a/b.svg", null, "alt", null, null, null, "  ", "k" },
        // sumber wajib kecuali karya sendiri
        { "sourceName", MediaKind.Image, "/media/a/b.svg", null, "alt", null, null, null, "CC BY 4.0", "k" },
        { "sourceName", MediaKind.Image, "/media/a/b.svg", null, "alt", null, "Penulis", null, "CC BY 4.0", "k" },
        // url sumber
        { "sourceUrl", MediaKind.Image, "/media/a/b.svg", null, "alt", null, "Penulis", "ftp://x.test", "CC BY 4.0", "k" },
        { "sourceUrl", MediaKind.Image, "/media/a/b.svg", null, "alt", null, "Penulis", "relatif/saja", "CC BY 4.0", "k" },
    };

    [Theory]
    [MemberData(nameof(Cacat))]
    public void UpsertMedia_MenolakBentukYangCacat_DenganNamaMedanYangKeliru_TanpaMengubahApaPun(
        string medan, MediaKind kind, string? url, string? videoId, string alt, string? caption, string? sourceName, string? sourceUrl, string license, string key)
    {
        var t = Ditinjau();

        var galat = Assert.Throws<ArgumentException>(
            () => t.UpsertMedia(key, kind, url, videoId, alt, caption, sourceName, sourceUrl, license));

        Assert.Equal(medan, galat.ParamName);
        Assert.Empty(t.Media);
        AssertTetapDitinjau(t);
    }

    [Fact]
    public void UpsertMedia_MenolakTeksKepanjangan()
    {
        var t = Ditinjau();

        Assert.Equal("alt", Assert.Throws<ArgumentException>(
            () => t.UpsertMedia("a", MediaKind.Image, "/media/a/b.svg", null, new string('x', TechnologyMedia.MaxAltLength + 1), null, null, null, TechnologyMedia.OwnWork)).ParamName);
        Assert.Equal("caption", Assert.Throws<ArgumentException>(
            () => t.UpsertMedia("a", MediaKind.Image, "/media/a/b.svg", null, "alt", new string('x', TechnologyMedia.MaxCaptionLength + 1), null, null, TechnologyMedia.OwnWork)).ParamName);
        Assert.Equal("license", Assert.Throws<ArgumentException>(
            () => t.UpsertMedia("a", MediaKind.Image, "/media/a/b.svg", null, "alt", null, null, null, new string('x', TechnologyMedia.MaxLicenseLength + 1))).ParamName);
        Assert.Equal("key", Assert.Throws<ArgumentException>(
            () => t.UpsertMedia(new string('k', TechnologyMedia.MaxKeyLength + 1), MediaKind.Image, "/media/a/b.svg", null, "alt", null, null, null, TechnologyMedia.OwnWork)).ParamName);

        Assert.Empty(t.Media);
    }

    [Fact]
    public void UpsertMedia_MembatasJumlahPerTopik_TapiMenggantiYangAdaTetapBoleh()
    {
        var t = Technology.Create("Topik", "Ringkasan.", AnyField);

        for (var i = 0; i < Technology.MaxMediaPerTopic; i++)
        {
            Gambar(t, $"gambar-{i}", $"Gambar {i}", $"/media/uji/{i}.svg");
        }

        var galat = Assert.Throws<InvalidOperationException>(() => Gambar(t, "kelebihan", "Kelebihan", "/media/uji/lebih.svg"));
        Assert.Contains(Technology.MaxMediaPerTopic.ToString(), galat.Message, StringComparison.Ordinal);
        Assert.Equal(Technology.MaxMediaPerTopic, t.Media.Count);

        Gambar(t, "gambar-3", "Gambar tiga, diganti", "/media/uji/3.svg"); // mengganti, bukan menambah
        Assert.Equal(Technology.MaxMediaPerTopic, t.Media.Count);
    }

    // ---- menggugurkan tinjau: mengganti ya; menambah, membuang, mengulang tidak ------------

    [Fact]
    public void MenambahMedia_TidakMenggugurkanPemeriksaan()
    {
        var t = Ditinjau();

        Gambar(t);

        AssertTetapDitinjau(t);
        Assert.Single(t.Media);
    }

    [Fact]
    public void MengulangMediaYangSama_TidakMengubahApaPun_BahkanUpdatedAt()
    {
        var t = Ditinjau();
        Gambar(t);
        t.MarkReviewed("pemilik");
        t.ClearEvents();
        var diperbarui = t.UpdatedAt;
        var id = t.Media[0].Id;

        t.UpsertMedia("arsitektur", MediaKind.Image, "/media/mcp/arsitektur.svg", null, "  Diagram arsitektur ", "Keterangan.", null, null, "karya sendiri");

        AssertTetapDitinjau(t);
        Assert.Equal(diperbarui, t.UpdatedAt);
        Assert.Equal(id, t.Media[0].Id);
    }

    [Theory]
    [InlineData("alt")]
    [InlineData("url")]
    [InlineData("caption")]
    [InlineData("jenis")]
    public void MenggantiMedia_ApaPunYangBerbeda_MenggugurkanPemeriksaan_DanBarisnyaTetap(string yangDiubah)
    {
        var t = Ditinjau();
        Gambar(t);
        t.MarkReviewed("pemilik");
        t.ClearEvents();
        var id = t.Media[0].Id;

        switch (yangDiubah)
        {
            case "alt":
                t.UpsertMedia("arsitektur", MediaKind.Image, "/media/mcp/arsitektur.svg", null, "Teks alternatif lain", "Keterangan.", null, null, TechnologyMedia.OwnWork);
                break;
            case "url":
                // Gambar diganti, teks alternatif sama: pemeriksa menyetujui alt untuk gambar ITU.
                t.UpsertMedia("arsitektur", MediaKind.Image, "/media/mcp/gambar-lain.svg", null, "Diagram arsitektur", "Keterangan.", null, null, TechnologyMedia.OwnWork);
                break;
            case "caption":
                t.UpsertMedia("arsitektur", MediaKind.Image, "/media/mcp/arsitektur.svg", null, "Diagram arsitektur", "Keterangan lain.", null, null, TechnologyMedia.OwnWork);
                break;
            default:
                t.UpsertMedia("arsitektur", MediaKind.Video, null, "dQw4w9WgXcQ", "Video pengganti", null, "Kanal", "https://x.test", "Hak cipta kanal");
                break;
        }

        AssertDigugurkan(t);
        var m = Assert.Single(t.Media);
        Assert.Equal(id, m.Id);
        Assert.Equal("arsitektur", m.Key);
    }

    [Fact]
    public void MembuangMedia_TidakMenggugurkanPemeriksaan_DanIdempoten()
    {
        var t = Ditinjau();
        Gambar(t);
        t.MarkReviewed("pemilik");
        t.ClearEvents();

        t.RemoveMedia("arsitektur");
        t.RemoveMedia("arsitektur");
        t.RemoveMedia("tak-pernah-ada");

        Assert.Empty(t.Media);
        AssertTetapDitinjau(t);
    }
}
