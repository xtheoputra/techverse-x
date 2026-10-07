using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using TechVerseX.Contracts.Technology;
using TechVerseX.TechnologyService.Infrastructure.Persistence;

namespace TechVerseX.Api.Tests;

/// <summary>
/// Media (gambar, diagram, video) dan <c>overview</c> berformat, lewat HTTP — ADR-028 Tahap 3b.
/// </summary>
/// <remarks>
/// 🔑 Tiga janji dijaga bersama kendali arah berlawanannya: (1) <b>media tanpa asal-usul tak bisa
/// masuk</b> — lewat API membalas 400 bermedan, dan lewat SQL mentah ditolak <c>CHECK</c> basis
/// data; (2) <b>mengganti</b> menggugurkan <c>tinjau</c> dan tersimpan, sedangkan menambah,
/// membuang, dan mengulang yang sama tidak; (3) PUT yang diulang idempoten — satu baris, tak ada
/// 500 dari indeks unik (jebakan <c>Include</c> yang terlewat).
/// <para>
/// ⚠️ Menyentuh PostgreSQL sungguhan, dan membersihkan barisnya sendiri.
/// </para>
/// </remarks>
public sealed class MediaDanOverviewEndpointTests : IAsyncLifetime
{
    private const string Postgres =
        "Host=localhost;Port=5432;Database=techversex;Username=techversex;Password=techversex_dev";

    private readonly List<string> _topikDibuat = [];
    private readonly List<string> _alatDibuat = [];

    private static WebApplicationFactory<Program> Host() =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:Postgres", Postgres);
            builder.UseSetting("ConnectionStrings:Redis", string.Empty);
            builder.UseSetting("Editorial:WritesEnabled", "true");
        });

    private static string Unik(string prefix) => $"{prefix}-{Guid.NewGuid():N}";

    private static Uri Alamat(string path) => new(path, UriKind.Relative);

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        if (_topikDibuat.Count == 0 && _alatDibuat.Count == 0)
        {
            return;
        }

        using var host = Host();
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TechnologyDbContext>();

        var topik = await db.Technologies.Where(t => _topikDibuat.Contains(t.Slug)).ToListAsync();
        db.Technologies.RemoveRange(topik);
        await db.SaveChangesAsync();

        var alat = await db.Tools.Where(t => _alatDibuat.Contains(t.Slug)).ToListAsync();
        db.Tools.RemoveRange(alat);
        await db.SaveChangesAsync();
    }

    private static MediaRequest Gambar(string alt = "Diagram arsitektur tiga peran", string? caption = "Host, client, server.", string url = "/media/uji/arsitektur.svg")
        => new("Image", url, null, alt, caption, null, null, "Karya sendiri");

    private static MediaRequest Video()
        => new("Video", null, "dQw4w9WgXcQ", "Demo resmi", null, "Kanal resmi", "https://www.youtube.com/@kanal", "Hak cipta pemilik kanal");

    // ---- menetapkan dan membaca -----------------------------------------------

    [Fact]
    public async Task Put_media_gambar_dan_video_tersimpan_dan_terbaca_lewat_GET()
    {
        using var host = Host();
        using var client = host.CreateClient();
        var (slug, _) = await IsiSampaiDrafAsync(client);

        Assert.Equal(HttpStatusCode.OK, (await client.PutAsJsonAsync(Alamat($"/api/v1/technologies/{slug}/media/arsitektur"), Gambar())).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.PutAsJsonAsync(Alamat($"/api/v1/technologies/{slug}/media/demo"), Video())).StatusCode);

        var dibaca = await client.GetFromJsonAsync<TechnologyResponse>(Alamat($"/api/v1/technologies/{slug}"));

        Assert.Equal(["arsitektur", "demo"], dibaca!.Media.Select(m => m.Key));

        var gambar = dibaca.Media[0];
        Assert.Equal("Image", gambar.Kind);
        Assert.Equal("/media/uji/arsitektur.svg", gambar.Url);
        Assert.Null(gambar.VideoId);
        Assert.Equal("Diagram arsitektur tiga peran", gambar.Alt);
        Assert.Equal("Karya sendiri", gambar.License);

        var video = dibaca.Media[1];
        Assert.Equal("Video", video.Kind);
        Assert.Equal("dQw4w9WgXcQ", video.VideoId);
        Assert.Null(video.Url);
        Assert.Equal("https://www.youtube.com/@kanal", video.SourceUrl);

        // Media menghias, ia bukan bagian template: kelima bagian tetap lengkap.
        Assert.Empty(dibaca.MissingSections);
    }

    [Fact]
    public async Task Put_media_yang_diulang_idempoten_satu_baris_tanpa_500_dan_UpdatedAt_tak_bergerak()
    {
        using var host = Host();
        using var client = host.CreateClient();
        var (slug, _) = await IsiSampaiDrafAsync(client);
        var alamat = Alamat($"/api/v1/technologies/{slug}/media/arsitektur");

        Assert.Equal(HttpStatusCode.OK, (await client.PutAsJsonAsync(alamat, Gambar())).StatusCode);

        // Dibandingkan dengan HASIL BACA basis data, bukan respons PUT pertama: respons itu
        // membawa UpdatedAt dari memori (presisi 100 ns) sedangkan PostgreSQL menyimpan
        // mikrodetik — selisih digit terakhir membuat tes ini tak stabil (diukur: …1113391 vs …1113390).
        var pertama = await Baca(client, slug);

        var kedua = await client.PutAsJsonAsync(alamat, Gambar());

        // Tanpa Include(Media) di jalur mutasi, ini 500 dari indeks unik (TechnologyId, Key).
        Assert.Equal(HttpStatusCode.OK, kedua.StatusCode);

        var sesudah = await kedua.Content.ReadFromJsonAsync<TechnologyResponse>();
        Assert.Single(sesudah!.Media);
        Assert.Equal(pertama.UpdatedAt, sesudah.UpdatedAt);
    }

    // ---- menggugurkan tinjau ----------------------------------------------------

    [Fact]
    public async Task Mengganti_media_menggugurkan_tinjau_dan_tersimpan_sedangkan_menambah_dan_membuang_tidak()
    {
        using var host = Host();
        using var client = host.CreateClient();
        var (slug, _) = await IsiSampaiTinjauAsync(client);
        var dasar = $"/api/v1/technologies/{slug}/media";

        // Menambah: tetap tinjau.
        Assert.Equal(HttpStatusCode.OK, (await client.PutAsJsonAsync(Alamat($"{dasar}/arsitektur"), Gambar())).StatusCode);
        Assert.Equal("HumanReviewed", (await Baca(client, slug)).Maturity);

        // Mengulang yang sama: tetap tinjau.
        Assert.Equal(HttpStatusCode.OK, (await client.PutAsJsonAsync(Alamat($"{dasar}/arsitektur"), Gambar())).StatusCode);
        Assert.Equal("HumanReviewed", (await Baca(client, slug)).Maturity);

        // Membuang: tetap tinjau, dan idempoten.
        Assert.Equal(HttpStatusCode.OK, (await client.PutAsJsonAsync(Alamat($"{dasar}/sementara"), Gambar(url: "/media/uji/sementara.svg"))).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.DeleteAsync(Alamat($"{dasar}/sementara"))).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.DeleteAsync(Alamat($"{dasar}/sementara"))).StatusCode);
        var masihTinjau = await Baca(client, slug);
        Assert.Equal("HumanReviewed", masihTinjau.Maturity);
        Assert.Equal(["arsitektur"], masihTinjau.Media.Select(m => m.Key));

        // Mengganti teks alternatif: gugur, dan tersimpan (DB, bukan hanya respons).
        Assert.Equal(HttpStatusCode.OK, (await client.PutAsJsonAsync(Alamat($"{dasar}/arsitektur"), Gambar(alt: "Teks alternatif yang lain"))).StatusCode);

        var gugur = await Baca(client, slug);
        Assert.Equal("MachineDrafted", gugur.Maturity);
        Assert.Null(gugur.ReviewedAt);
        Assert.Equal("Teks alternatif yang lain", Assert.Single(gugur.Media).Alt);
        Assert.Null(await PemeriksaDiDbAsync(host, slug));
    }

    // ---- validasi: 400 bermedan, bukan 500 --------------------------------------------

    public static TheoryData<string, MediaRequest, string> Cacat => new()
    {
        { "arsitektur", Gambar(alt: "   "), "alt" },
        { "arsitektur", new MediaRequest("Image", "/media/uji/a.svg", null, "Alt", null, null, null, "  "), "license" },
        { "arsitektur", new MediaRequest("Image", "/media/uji/a.svg", null, "Alt", null, null, null, "CC BY 4.0"), "sourceName" },
        { "arsitektur", new MediaRequest("Image", "/media/uji/a.svg", null, "Alt", null, "Penulis", "ftp://x.test", "CC BY 4.0"), "sourceUrl" },
        { "arsitektur", Gambar(url: "https://x.test/hotlink.png"), "url" },
        { "arsitektur", Gambar(url: "/media/../rahasia.svg"), "url" },
        { "arsitektur", new MediaRequest("Image", null, null, "Alt", null, null, null, "Karya sendiri"), "url" },
        { "demo", new MediaRequest("Video", null, "pendek", "Judul", null, "Kanal", "https://x.test", "Lisensi"), "videoId" },
        { "demo", new MediaRequest("Video", "/media/uji/a.svg", "dQw4w9WgXcQ", "Judul", null, "Kanal", "https://x.test", "Lisensi"), "url" },
        { "arsitektur", new MediaRequest("Gif", "/media/uji/a.svg", null, "Alt", null, null, null, "Karya sendiri"), "kind" },
        { "arsitektur", new MediaRequest("0", "/media/uji/a.svg", null, "Alt", null, null, null, "Karya sendiri"), "kind" },
        { "Besar", Gambar(), "key" },
        { "dua-kata-ini-benar-tapi-terlalu-panjang-sekali-sampai-melewati-batas-kunci-yang-delapan-puluh", Gambar(), "key" },
        { "arsitektur", Gambar(alt: new string('a', 501)), "alt" },
        { "arsitektur", Gambar(caption: new string('c', 1001)), "caption" },
    };

    [Theory]
    [MemberData(nameof(Cacat))]
    public async Task Put_media_yang_cacat_membalas_400_bermedan_yang_keliru_dan_tak_menyimpan_apa_pun(string kunci, MediaRequest isi, string medan)
    {
        using var host = Host();
        using var client = host.CreateClient();
        var (slug, _) = await IsiSampaiDrafAsync(client);

        var response = await client.PutAsJsonAsync(Alamat($"/api/v1/technologies/{slug}/media/{kunci}"), isi);

        var badan = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest, $"harus 400, bukan {(int)response.StatusCode}. Badan: {badan}");

        using var dokumen = JsonDocument.Parse(badan);
        Assert.True(
            dokumen.RootElement.TryGetProperty("errors", out var errors) && errors.TryGetProperty(medan, out _),
            $"400-nya harus bermedan '{medan}'. Badan: {badan}");

        Assert.Empty((await Baca(client, slug)).Media);
    }

    [Fact]
    public async Task Media_pada_topik_yang_tidak_ada_membalas_404_baik_PUT_maupun_DELETE()
    {
        using var host = Host();
        using var client = host.CreateClient();

        Assert.Equal(HttpStatusCode.NotFound, (await client.PutAsJsonAsync(Alamat($"/api/v1/technologies/{Unik("tidak-ada")}/media/a"), Gambar())).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.DeleteAsync(Alamat($"/api/v1/technologies/{Unik("tidak-ada")}/media/a"))).StatusCode);
    }

    [Fact]
    public async Task Media_dibatasi_per_topik_dan_kelebihannya_400_bukan_500()
    {
        using var host = Host();
        using var client = host.CreateClient();
        var (slug, _) = await IsiSampaiDrafAsync(client);

        for (var i = 0; i < 40; i++)
        {
            Assert.Equal(HttpStatusCode.OK, (await client.PutAsJsonAsync(
                Alamat($"/api/v1/technologies/{slug}/media/gambar-{i}"), Gambar(url: $"/media/uji/{i}.svg"))).StatusCode);
        }

        var lebih = await client.PutAsJsonAsync(Alamat($"/api/v1/technologies/{slug}/media/kelebihan"), Gambar(url: "/media/uji/lebih.svg"));

        Assert.Equal(HttpStatusCode.BadRequest, lebih.StatusCode);
        Assert.Contains("40", await lebih.Content.ReadAsStringAsync(), StringComparison.Ordinal);
        Assert.Equal(40, (await Baca(client, slug)).Media.Count);
    }

    // ---- overview -----------------------------------------------------------------------

    [Fact]
    public async Task Overview_lewat_PUT_topik_tersimpan_dikosongkan_bila_tak_dikirim_dan_menggugurkan_tinjau_hanya_bila_berbeda()
    {
        using var host = Host();
        using var client = host.CreateClient();
        var (slug, _) = await IsiSampaiTinjauAsync(client);
        var alamat = Alamat($"/api/v1/technologies/{slug}");

        // Menetapkan overview pertama kali menggugurkan tinjau (teks yang dibaca pemeriksa berubah).
        var tetap = await client.PutAsJsonAsync(alamat, new UpdateTechnologyRequest("Uji Jalan Ubah", "Ringkasan untuk uji.", "## Mengapa\n\nPendalaman."));
        Assert.Equal(HttpStatusCode.OK, tetap.StatusCode);
        var sesudah = await Baca(client, slug);
        Assert.Equal("## Mengapa\n\nPendalaman.", sesudah.Overview);
        Assert.Equal("MachineDrafted", sesudah.Maturity);

        // Diperiksa lagi, lalu PUT yang SAMA (spasi di tepi): tetap tinjau.
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync(Alamat($"/api/v1/technologies/{slug}/tinjau"), new MarkReviewedRequest("Harasta"))).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.PutAsJsonAsync(alamat, new UpdateTechnologyRequest("Uji Jalan Ubah", "Ringkasan untuk uji.", "  ## Mengapa\n\nPendalaman.\n"))).StatusCode);
        Assert.Equal("HumanReviewed", (await Baca(client, slug)).Maturity);

        // Tidak dikirim = dikosongkan, dan itu perubahan: gugur.
        Assert.Equal(HttpStatusCode.OK, (await client.PutAsJsonAsync(alamat, new UpdateTechnologyRequest("Uji Jalan Ubah", "Ringkasan untuk uji."))).StatusCode);
        var kosong = await Baca(client, slug);
        Assert.Null(kosong.Overview);
        Assert.Equal("MachineDrafted", kosong.Maturity);
    }

    [Fact]
    public async Task Overview_kepanjangan_membalas_400_bermedan_overview_bukan_500()
    {
        using var host = Host();
        using var client = host.CreateClient();
        var (slug, _) = await IsiSampaiDrafAsync(client);

        var response = await client.PutAsJsonAsync(
            Alamat($"/api/v1/technologies/{slug}"),
            new UpdateTechnologyRequest("Uji Jalan Ubah", "Ringkasan untuk uji.", new string('x', 20001)));

        var badan = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var dokumen = JsonDocument.Parse(badan);
        Assert.True(dokumen.RootElement.GetProperty("errors").TryGetProperty("overview", out _), badan);
    }

    // ---- CHECK basis data: jalan tulis yang tak lewat domain ------------------------------

    [Theory]
    [InlineData("gambar tanpa berkas", "Image", null, null, "Alt", "Karya sendiri", null, null, "ck_technology_media_bentuk")]
    [InlineData("gambar bawa videoId", "Image", "/media/x/a.svg", "dQw4w9WgXcQ", "Alt", "Karya sendiri", null, null, "ck_technology_media_bentuk")]
    [InlineData("video tanpa id", "Video", null, null, "Judul", "Lisensi", "Kanal", "https://x.test", "ck_technology_media_bentuk")]
    [InlineData("gambar hotlink", "Image", "https://x.test/a.png", null, "Alt", "Karya sendiri", null, null, "ck_technology_media_berkas_sendiri")]
    [InlineData("alt kosong", "Image", "/media/x/a.svg", null, "   ", "Karya sendiri", null, null, "ck_technology_media_alt")]
    [InlineData("lisensi kosong", "Image", "/media/x/a.svg", null, "Alt", "  ", null, null, "ck_technology_media_lisensi")]
    [InlineData("tanpa sumber", "Image", "/media/x/a.svg", null, "Alt", "CC BY 4.0", null, null, "ck_technology_media_lisensi")]
    // PostgreSQL memeriksa CHECK berurut nama, dan jenis yang bukan Image/Video melanggar `bentuk`
    // lebih dulu daripada `kind` — jadi `kind` tak pernah menjadi penolak pertama. Ia tetap ada:
    // daftarnya dibangkitkan dari enum, jadi anggota MediaKind baru memerahkan ModelMigrasiTests
    // sampai migrasinya dibuat (sama dengan CHECK jenis relasi, ADR-023).
    [InlineData("jenis liar", "Gif", "/media/x/a.svg", null, "Alt", "Karya sendiri", null, null, "ck_technology_media_bentuk")]
    public async Task Basis_data_menolak_media_cacat_walau_lewat_SQL_mentah(
        string nama, string kind, string? url, string? videoId, string alt, string license, string? sourceName, string? sourceUrl, string constraint)
    {
        using var host = Host();
        using var client = host.CreateClient();
        var (slug, _) = await IsiSampaiDrafAsync(client);
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TechnologyDbContext>();
        var topikId = (await db.Technologies.AsNoTracking().SingleAsync(t => t.Slug == slug)).Id;

        var galat = await Assert.ThrowsAsync<PostgresException>(() => SisipMentahAsync(db, topikId, "kunci", kind, url, videoId, alt, license, sourceName, sourceUrl));

        Assert.True(galat.ConstraintName == constraint, $"[{nama}] diharapkan {constraint}, dapat {galat.ConstraintName}: {galat.MessageText}");
    }

    [Fact]
    public async Task Basis_data_menolak_kunci_ganda_dan_kunci_berbentuk_salah_dan_menerima_yang_sah()
    {
        using var host = Host();
        using var client = host.CreateClient();
        var (slug, _) = await IsiSampaiDrafAsync(client);
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TechnologyDbContext>();
        var topikId = (await db.Technologies.AsNoTracking().SingleAsync(t => t.Slug == slug)).Id;

        // Kendali: bentuk yang sah MASUK — tanpa ini semua penolakan di atas bisa hijau karena
        // pernyataan SQL-nya sendiri yang salah.
        await SisipMentahAsync(db, topikId, "sah", "Image", "/media/x/a.svg", null, "Alt", "Karya sendiri", null, null);

        var ganda = await Assert.ThrowsAsync<PostgresException>(
            () => SisipMentahAsync(db, topikId, "sah", "Image", "/media/x/b.svg", null, "Alt", "Karya sendiri", null, null));
        Assert.Equal("ix_technology_media_technology_key", ganda.ConstraintName);

        var salah = await Assert.ThrowsAsync<PostgresException>(
            () => SisipMentahAsync(db, topikId, "Kunci Besar", "Image", "/media/x/c.svg", null, "Alt", "Karya sendiri", null, null));
        Assert.Equal("ck_technology_media_kunci", salah.ConstraintName);
    }

    [Fact]
    public async Task Membuang_topik_membuang_medianya_dan_tak_meninggalkan_baris_yatim()
    {
        using var host = Host();
        using var client = host.CreateClient();
        var (slug, _) = await IsiSampaiDrafAsync(client);
        Assert.Equal(HttpStatusCode.OK, (await client.PutAsJsonAsync(Alamat($"/api/v1/technologies/{slug}/media/arsitektur"), Gambar())).StatusCode);

        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TechnologyDbContext>();
        var topik = await db.Technologies.SingleAsync(t => t.Slug == slug);
        var topikId = topik.Id;

        Assert.Equal(1, await HitungMediaAsync(db, topikId));

        db.Technologies.Remove(topik);
        await db.SaveChangesAsync();
        _topikDibuat.Remove(slug); // sudah dibuang di sini; pembersihan akhir tak perlu mencarinya lagi

        Assert.Equal(0, await HitungMediaAsync(db, topikId));
    }

    // ---- alat bantu ------------------------------------------------------------------------

    private static async Task<TechnologyResponse> Baca(HttpClient client, string slug)
        => (await client.GetFromJsonAsync<TechnologyResponse>(Alamat($"/api/v1/technologies/{slug}")))!;

    private static async Task<string?> PemeriksaDiDbAsync(WebApplicationFactory<Program> host, string slug)
    {
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TechnologyDbContext>();
        return (await db.Technologies.AsNoTracking().SingleAsync(t => t.Slug == slug)).ReviewedBy;
    }

    private static Task<int> SisipMentahAsync(
        TechnologyDbContext db, Guid topikId, string key, string kind, string? url, string? videoId,
        string alt, string license, string? sourceName, string? sourceUrl)
        => db.Database.ExecuteSqlRawAsync(
            "INSERT INTO technology.technology_media (\"Id\", \"TechnologyId\", \"Key\", \"Kind\", \"Url\", \"VideoId\", \"Alt\", \"License\", \"SourceName\", \"SourceUrl\", \"CreatedAt\") "
            + "VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, now())",
            Guid.CreateVersion7(), topikId, key, kind, url!, videoId!, alt, license, sourceName!, sourceUrl!);

    private static async Task<int> HitungMediaAsync(TechnologyDbContext db, Guid topikId)
    {
        var koneksi = db.Database.GetDbConnection();
        await koneksi.OpenAsync();
        try
        {
            using var perintah = koneksi.CreateCommand();
            perintah.CommandText = "SELECT count(*) FROM technology.technology_media WHERE \"TechnologyId\" = @id";
            var parameter = perintah.CreateParameter();
            parameter.ParameterName = "id";
            parameter.Value = topikId;
            perintah.Parameters.Add(parameter);
            return Convert.ToInt32(await perintah.ExecuteScalarAsync(), System.Globalization.CultureInfo.InvariantCulture);
        }
        finally
        {
            await koneksi.CloseAsync();
        }
    }

    private async Task<string> BuatTopikAsync(HttpClient client)
    {
        var slug = Unik("uji-media");

        var response = await client.PostAsJsonAsync(
            Alamat("/api/v1/technologies"),
            new CreateTechnologyRequest("Uji Jalan Ubah", "Ringkasan untuk uji.", "ai-agents", slug));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        _topikDibuat.Add(slug);
        return slug;
    }

    private async Task<(string Slug, string Alat)> IsiSampaiDrafAsync(HttpClient client)
    {
        var slug = await BuatTopikAsync(client);

        Assert.Equal(HttpStatusCode.OK, (await client.PutAsJsonAsync(
            Alamat($"/api/v1/technologies/{slug}/roadmap/prasyarat"),
            new RoadmapStepRequest("Dasar pemrograman", "Bisa menulis fungsi."))).StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync(
            Alamat($"/api/v1/technologies/{slug}/roadmap"),
            new RoadmapStepRequest("Langkah pertama", "Menjalankan contoh."))).StatusCode);

        var alat = Unik("alat");
        _alatDibuat.Add(alat);
        Assert.Equal(HttpStatusCode.Created, (await client.PostAsJsonAsync(
            Alamat("/api/v1/tools"),
            new CreateToolRequest("Alat Uji", "Dipakai uji integrasi.", "https://example.com", alat))).StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync(
            Alamat($"/api/v1/technologies/{slug}/tools"),
            new AttachToolRequest(alat, "Dipakai di langkah pertama."))).StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync(
            Alamat($"/api/v1/technologies/{slug}/projects"),
            new ProjectRequest("Proyek kecil", "Membangun contoh minimal."))).StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync(
            Alamat($"/api/v1/technologies/{slug}/resources"),
            new ResourceRequest("OfficialDocs", "Dokumentasi resmi", "https://example.com/docs"))).StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await client.PostAsync(Alamat($"/api/v1/technologies/{slug}/draf"), null)).StatusCode);

        return (slug, alat);
    }

    private async Task<(string Slug, string Alat)> IsiSampaiTinjauAsync(HttpClient client)
    {
        var hasil = await IsiSampaiDrafAsync(client);

        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync(
            Alamat($"/api/v1/technologies/{hasil.Slug}/tinjau"),
            new MarkReviewedRequest("Harasta"))).StatusCode);

        return hasil;
    }
}
