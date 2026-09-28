using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TechVerseX.Contracts.Technology;
using TechVerseX.TechnologyService.Infrastructure.Persistence;

namespace TechVerseX.Api.Tests;

/// <summary>
/// Kelima bagian template ADR-012 lewat HTTP — dari topik kosong sampai
/// <c>draf</c>.
/// </summary>
/// <remarks>
/// 🔑 <b>Yang dijaga berkas ini bukan "endpointnya ada", melainkan bahwa muatan
/// yang keliru membalas 400 dan bukan 500.</b> Itu pelajaran issue #26, dan ia
/// mahal justru karena dulu hanya berlaku di sebagian jalur. Di sini tiga jalur
/// salah diuji sekaligus: urutan yang salah (langkah roadmap sebelum prasyarat),
/// enum yang tidak dikenal, dan URL yang bukan http.
/// <para>
/// ⚠️ Menyentuh PostgreSQL sungguhan (<c>run.ps1 up</c> lalu <c>run.ps1 migrate</c>
/// di lokal, service container di CI). Tiap uji memakai slug berakhiran GUID
/// supaya jalannya tidak bergantung pada basis data yang bersih — dan supaya dua
/// uji tidak saling menjatuhkan lewat indeks unik slug.
/// </para>
/// <para>
/// 🧹 <b>Barisnya dibersihkan lagi di <see cref="DisposeAsync"/>.</b> Di CI hal ini
/// tidak terasa — service container lahir kosong tiap jalan — tapi di mesin
/// pengembang basis datanya sama dengan yang dipakai <c>run.ps1 web</c>. Tanpa
/// pembersihan, satu sore menjalankan uji meninggalkan puluhan topik "Uji Isi
/// Halaman" di halaman bidang yang sungguhan. Terlihat saat halamannya dibuka,
/// bukan saat ujinya dijalankan.
/// </para>
/// </remarks>
public sealed class ContentSectionEndpointTests : IAsyncLifetime
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

            // 🔴 Tanpa baris ini SELURUH berkas ini gagal: endpoint tulis tidak
            // dipasang kecuali diminta (ADR-020), dan WebApplicationFactory tidak
            // pernah membaca launchSettings.json. Bahwa baris ini WAJIB ditulis
            // di sini adalah bagian dari buktinya — bentuk bawaannya tertutup.
            builder.UseSetting("Editorial:WritesEnabled", "true");
        });

    private static string Unik(string prefix) => $"{prefix}-{Guid.NewGuid():N}";

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

        // Topik lebih dulu: keempat bagian isinya ikut terbuang lewat cascade,
        // dan technology_tools harus hilang sebelum alatnya boleh dibuang -
        // sisi Tool sengaja Restrict.
        var topik = await db.Technologies.Where(t => _topikDibuat.Contains(t.Slug)).ToListAsync();
        db.Technologies.RemoveRange(topik);
        await db.SaveChangesAsync();

        var alat = await db.Tools.Where(t => _alatDibuat.Contains(t.Slug)).ToListAsync();
        db.Tools.RemoveRange(alat);
        await db.SaveChangesAsync();
    }

    private async Task<string> BuatTopikAsync(HttpClient client)
    {
        var slug = Unik("uji-isi");

        var response = await client.PostAsJsonAsync(
            new Uri("/api/v1/technologies", UriKind.Relative),
            new CreateTechnologyRequest("Uji Isi Halaman", "Ringkasan untuk uji.", "ai-agents", slug));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        _topikDibuat.Add(slug);
        return slug;
    }

    [Fact]
    public async Task Kelima_bagian_terisi_membuat_MissingSections_kosong_dan_draf_diterima()
    {
        using var host = Host();
        using var client = host.CreateClient();

        var slug = await BuatTopikAsync(client);
        var topik = new Uri($"/api/v1/technologies/{slug}", UriKind.Relative);

        // Bagian 1 (Overview) sudah terisi lewat Summary saat topiknya dibuat.
        var awal = await client.GetFromJsonAsync<TechnologyResponse>(topik);
        Assert.NotNull(awal);
        Assert.DoesNotContain("Overview", awal!.MissingSections);

        // 🔑 Roadmap yang HANYA berisi prasyarat belum dianggap terisi. Aturan itu
        // hidup di agregat; uji ini membuktikan ia sampai ke luar lewat HTTP.
        Assert.Equal(HttpStatusCode.OK, (await client.PutAsJsonAsync(
            new Uri($"/api/v1/technologies/{slug}/roadmap/prasyarat", UriKind.Relative),
            new RoadmapStepRequest("Dasar pemrograman", "Bisa menulis fungsi."))).StatusCode);

        var setelahPrasyarat = await client.GetFromJsonAsync<TechnologyResponse>(topik);
        Assert.Contains("Learning Roadmap", setelahPrasyarat!.MissingSections);

        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync(
            new Uri($"/api/v1/technologies/{slug}/roadmap", UriKind.Relative),
            new RoadmapStepRequest("Langkah pertama", "Menjalankan contoh."))).StatusCode);

        var toolSlug = Unik("alat");
        _alatDibuat.Add(toolSlug);
        Assert.Equal(HttpStatusCode.Created, (await client.PostAsJsonAsync(
            new Uri("/api/v1/tools", UriKind.Relative),
            new CreateToolRequest("Alat Uji", "Dipakai uji integrasi.", "https://example.com", toolSlug))).StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync(
            new Uri($"/api/v1/technologies/{slug}/tools", UriKind.Relative),
            new AttachToolRequest(toolSlug, "Dipakai di langkah pertama."))).StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync(
            new Uri($"/api/v1/technologies/{slug}/projects", UriKind.Relative),
            new ProjectRequest("Proyek kecil", "Membangun contoh minimal."))).StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync(
            new Uri($"/api/v1/technologies/{slug}/resources", UriKind.Relative),
            new ResourceRequest("OfficialDocs", "Dokumentasi resmi", "https://example.com/docs"))).StatusCode);

        var lengkap = await client.GetFromJsonAsync<TechnologyResponse>(topik);
        Assert.Empty(lengkap!.MissingSections);

        // Alat DITAUTKAN: yang kembali membawa identitas dari katalog, bukan
        // salinan yang tinggal di topik.
        var alat = Assert.Single(lengkap.Tools);
        Assert.Equal(toolSlug, alat.Slug);
        Assert.Equal("Dipakai di langkah pertama.", alat.Note);

        // Nomor langkah ditentukan server: prasyarat 0, lalu 1.
        Assert.Equal([0, 1], lengkap.Roadmap.Select(s => s.Order));
        Assert.True(lengkap.Roadmap[0].IsPrerequisite);

        var draf = await client.PostAsync(new Uri($"/api/v1/technologies/{slug}/draf", UriKind.Relative), null);
        Assert.Equal(HttpStatusCode.OK, draf.StatusCode);
        Assert.Equal("MachineDrafted", (await draf.Content.ReadFromJsonAsync<TechnologyResponse>())!.Maturity);
    }

    [Fact]
    public async Task Draf_ditolak_400_saat_bagian_masih_kosong_dan_menyebut_bagiannya()
    {
        using var host = Host();
        using var client = host.CreateClient();

        var slug = await BuatTopikAsync(client);

        var response = await client.PostAsync(
            new Uri($"/api/v1/technologies/{slug}/draf", UriKind.Relative), null);

        // 400, bukan 500 — dan bukan pula 200 yang meluluskan halaman kosong.
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Learning Roadmap", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Langkah_roadmap_sebelum_prasyarat_membalas_400_bukan_500()
    {
        using var host = Host();
        using var client = host.CreateClient();

        var slug = await BuatTopikAsync(client);

        var response = await client.PostAsJsonAsync(
            new Uri($"/api/v1/technologies/{slug}/roadmap", UriKind.Relative),
            new RoadmapStepRequest("Langkah tanpa prasyarat", "Seharusnya ditolak."));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("SetPrerequisite", await response.Content.ReadAsStringAsync(), StringComparison.Ordinal);
    }

    [Theory]
    // Enum yang tidak dikenal - tidak boleh diam-diam jatuh ke anggota pertama.
    [InlineData("Vidio", "https://example.com/x")]
    // Skema yang tidak punya urusan di daftar sumber belajar.
    [InlineData("Video", "javascript:alert(1)")]
    // Tautan relatif: lolos ke halaman lalu mati di peramban pembaca.
    [InlineData("Video", "/relatif")]
    public async Task Sumber_yang_cacat_membalas_400_bukan_500(string type, string url)
    {
        using var host = Host();
        using var client = host.CreateClient();

        var slug = await BuatTopikAsync(client);

        var response = await client.PostAsJsonAsync(
            new Uri($"/api/v1/technologies/{slug}/resources", UriKind.Relative),
            new ResourceRequest(type, "Sumber cacat", url));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// Jenis sumber diurai berdasarkan NAMA saja — issue #60.
    /// </summary>
    /// <remarks>
    /// 🔴 Dibuktikan merah lebih dulu (2026-09-17): dengan <c>Enum.TryParse</c>
    /// kelimanya membalas 200 dan TERSIMPAN — <c>"0"</c> sebagai
    /// <c>OfficialDocs</c>, <c>"3"</c> sebagai <c>Repository</c>, <c>" 1 "</c> dan
    /// <c>" video "</c> sebagai <c>Video</c>, dan gabungan bendera
    /// <c>"Video, Paper"</c> sebagai <c>Repository</c> (1 | 2). Empat dari lima tidak
    /// menyebut nama anggota yang tersimpan, dan tak satu pun berbunyi.
    /// </remarks>
    [Theory]
    [InlineData("0")]
    [InlineData("3")]
    [InlineData(" 1 ")]
    [InlineData("Video, Paper")]
    // Spasi bukan bagian dari nama. Pesannya menyebut nama yang sah, jadi
    // penolakannya tidak membingungkan pemanggil.
    [InlineData(" video ")]
    public async Task Jenis_sumber_yang_bukan_nama_ditolak_400_bermedan_type_dan_tidak_tersimpan(string type)
    {
        using var host = Host();
        using var client = host.CreateClient();

        var slug = await BuatTopikAsync(client);

        var response = await client.PostAsJsonAsync(
            new Uri($"/api/v1/technologies/{slug}/resources", UriKind.Relative),
            new ResourceRequest(type, "Sumber berjenis angka", "https://example.com/angka"));

        var isi = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        // Medan `type` di dalam `errors` - bukan `"type"` di mana saja, sebab setiap
        // ProblemDetails membawa medan `type` di akarnya sendiri.
        using var dokumen = JsonDocument.Parse(isi);
        Assert.True(
            dokumen.RootElement.TryGetProperty("errors", out var errors) && errors.TryGetProperty("type", out _),
            $"400-nya harus bermedan 'type'. Badan: {isi}");

        var topik = await client.GetFromJsonAsync<TechnologyResponse>(
            new Uri($"/api/v1/technologies/{slug}", UriKind.Relative));
        Assert.Empty(topik!.Resources);
    }

    /// <summary>
    /// Kendali #60: nama tanpa peka huruf besar-kecil TETAP diterima. Tanpa ini,
    /// perbaikan yang menolak segalanya ikut hijau.
    /// </summary>
    [Theory]
    [InlineData("video", "Video")]
    [InlineData("REPOSITORY", "Repository")]
    [InlineData("OfficialDocs", "OfficialDocs")]
    public async Task Nama_jenis_sumber_diterima_tanpa_peka_huruf_besar_kecil(string type, string tersimpan)
    {
        using var host = Host();
        using var client = host.CreateClient();

        var slug = await BuatTopikAsync(client);

        var response = await client.PostAsJsonAsync(
            new Uri($"/api/v1/technologies/{slug}/resources", UriKind.Relative),
            new ResourceRequest(type, "Sumber bernama", "https://example.com/nama"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var sumber = Assert.Single((await response.Content.ReadFromJsonAsync<TechnologyResponse>())!.Resources);
        Assert.Equal(tersimpan, sumber.Type);
    }

    [Fact]
    public async Task Menautkan_alat_yang_belum_ada_di_katalog_membalas_400()
    {
        using var host = Host();
        using var client = host.CreateClient();

        var slug = await BuatTopikAsync(client);

        var response = await client.PostAsJsonAsync(
            new Uri($"/api/v1/technologies/{slug}/tools", UriKind.Relative),
            new AttachToolRequest("alat-yang-tidak-pernah-dibuat"));

        // 400, bukan 404: yang tidak ditemukan ada di MUATAN, bukan di alamatnya.
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Bagian_isi_pada_topik_yang_tidak_ada_membalas_404()
    {
        using var host = Host();
        using var client = host.CreateClient();

        var response = await client.PostAsJsonAsync(
            new Uri($"/api/v1/technologies/{Unik("tidak-ada")}/projects", UriKind.Relative),
            new ProjectRequest("Proyek", "Tidak akan tersimpan."));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    /// <summary>
    /// 🔴 Ruang nama <c>/teknologi/&lt;slug&gt;</c> dipakai BERSAMA bidang dan topik
    /// (ADR-009), tapi keunikannya dijaga dua indeks yang terpisah.
    /// </summary>
    /// <remarks>
    /// Dibuktikan sebelum diperbaiki: <c>POST</c> dengan slug <c>cybersecurity</c>
    /// — slug milik salah satu dari 14 bidang — membalas <b>201</b>, dan sejak itu
    /// alamat yang ADR-009 janjikan stabil punya dua pemilik.
    /// </remarks>
    [Fact]
    public async Task Topik_tidak_boleh_mengambil_slug_milik_bidang()
    {
        using var host = Host();
        using var client = host.CreateClient();

        var response = await client.PostAsJsonAsync(
            new Uri("/api/v1/technologies", UriKind.Relative),
            new CreateTechnologyRequest("Tabrakan Slug", "Sengaja memakai slug bidang.", "ai-agents", "cybersecurity"));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Contains("bidang", await response.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 🔴 <c>Location</c> di jawaban <c>201</c> adalah janji bahwa yang baru dibuat
    /// bisa dibaca di alamat itu — dan sampai 2026-09-28 satu dari dua janji itu
    /// palsu.
    /// </summary>
    /// <remarks>
    /// Diukur ke API pengembangan sebelum diperbaiki: <c>POST /api/v1/tools</c>
    /// membalas <b>201</b> dengan <c>Location: /api/v1/tools/&lt;slug&gt;</c>, dan
    /// alamat itu membalas <b>404</b> — tidak ada rute <c>GET</c> alat sama sekali.
    /// Pembaca <c>ToolResponse.Slug</c> satu-satunya di kode produksi adalah baris
    /// yang menyusun header itu.
    /// <para>
    /// Invariannya sengaja tidak menyebut rute mana pun: <em>kalau</em> ada
    /// <c>Location</c>, ia wajib terbaca. Topik jadi kendalinya — Location-nya
    /// DIPASTIKAN ada, jadi invarian ini tidak bisa hijau karena tidak pernah
    /// dijalankan. Menambah <c>GET /api/v1/tools/{slug}</c> kelak membuat
    /// Location alat sah lagi tanpa uji ini perlu diubah.
    /// </para>
    /// </remarks>
    [Fact]
    public async Task Location_di_jawaban_201_hanya_menunjuk_alamat_yang_bisa_dibaca()
    {
        using var host = Host();
        using var client = host.CreateClient();

        var slugTopik = Unik("uji-lokasi");
        _topikDibuat.Add(slugTopik);
        var topik = await client.PostAsJsonAsync(
            new Uri("/api/v1/technologies", UriKind.Relative),
            new CreateTechnologyRequest("Uji Lokasi", "Ringkasan untuk uji.", "ai-agents", slugTopik));
        Assert.Equal(HttpStatusCode.Created, topik.StatusCode);

        // Kendali: Location topik ada, dan membukanya menjawab 200.
        Assert.NotNull(topik.Headers.Location);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(topik.Headers.Location)).StatusCode);

        var slugAlat = Unik("alat-lokasi");
        _alatDibuat.Add(slugAlat);
        var alat = await client.PostAsJsonAsync(
            new Uri("/api/v1/tools", UriKind.Relative),
            new CreateToolRequest("Alat Lokasi", "Dipakai uji Location.", null, slugAlat));
        Assert.Equal(HttpStatusCode.Created, alat.StatusCode);

        if (alat.Headers.Location is { } lokasiAlat)
        {
            Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(lokasiAlat)).StatusCode);
        }

        // Tanpa Location pun jawabannya tetap menyebut apa yang dibuat.
        Assert.Equal(slugAlat, (await alat.Content.ReadFromJsonAsync<ToolResponse>())!.Slug);
    }
}
