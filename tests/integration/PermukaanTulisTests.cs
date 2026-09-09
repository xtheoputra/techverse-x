using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TechVerseX.Contracts.Technology;
using TechVerseX.TechnologyService.Infrastructure.Persistence;

namespace TechVerseX.Api.Tests;

/// <summary>
/// Permukaan TULIS redaksi tidak dipasang kecuali diminta — <b>ADR-020</b>.
/// </summary>
/// <remarks>
/// 🔴 <b>Kenapa gerbang ini ada.</b> V1 tidak punya autentikasi sama sekali
/// (ADR-013), dan ADR itu menimbang login untuk <em>pembaca</em> — Progress
/// Tracker, Badge, AI Mentor. Ia tidak pernah menimbang permukaan tulis
/// <em>redaksi</em>. Sampai 2026-09-09 seluruh endpoint tulis dipasang tanpa
/// syarat, jadi begitu API tayang di URL publik (issue #39/#40), siapa pun yang
/// menemukan alamatnya boleh membuat topik yang langsung muncul di halaman muka,
/// menyisipkan tautan asing di bagian Resources, dan menaikkan halaman ke
/// <c>draf</c>.
/// <para>
/// 🔑 <b>Berkas ini ditulis karena gerbang yang belum pernah terlihat merah belum
/// terbukti menjaga apa pun.</b> Dan sebuah uji yang hanya memastikan "404" bisa
/// hijau karena alasan yang keliru: URL yang salah ketik juga 404. Karena itu tiap
/// pemeriksaan di sini dijalankan terhadap <b>topik yang SUNGGUH ADA</b>, dan
/// disandingkan dengan kendali arah berlawanan — muatan dan alamat yang sama
/// persis, hanya sakelarnya yang berbeda.
/// </para>
/// <para>
/// ⚠️ Menyentuh PostgreSQL sungguhan (<c>run.ps1 up</c> lalu <c>run.ps1 migrate</c>
/// di lokal, service container di CI), dan membersihkan barisnya sendiri.
/// </para>
/// </remarks>
public sealed class PermukaanTulisTests : IAsyncLifetime
{
    private const string Postgres =
        "Host=localhost;Port=5432;Database=techversex;Username=techversex;Password=techversex_dev";

    private readonly List<string> _topikDibuat = [];
    private readonly List<string> _alatDibuat = [];

    /// <summary>
    /// Host apa adanya — <b>persis seperti produksi membangunnya</b>. Perhatikan
    /// yang TIDAK ada di sini: <c>Editorial:WritesEnabled</c>. Itulah bentuk
    /// bawaannya, dan itulah yang sedang diuji.
    /// </summary>
    private static WebApplicationFactory<Program> HostBawaan() =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:Postgres", Postgres);
            builder.UseSetting("ConnectionStrings:Redis", string.Empty);
        });

    private static WebApplicationFactory<Program> HostTerbuka() =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:Postgres", Postgres);
            builder.UseSetting("ConnectionStrings:Redis", string.Empty);
            builder.UseSetting("Editorial:WritesEnabled", "true");
        });

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        if (_topikDibuat.Count == 0 && _alatDibuat.Count == 0)
        {
            return;
        }

        using var host = HostBawaan();
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TechnologyDbContext>();

        // Topik lebih dulu, sama seperti ContentSectionEndpointTests: sisi Tool
        // sengaja Restrict, jadi tautannya harus hilang sebelum alatnya boleh
        // dibuang.
        var topik = await db.Technologies.Where(t => _topikDibuat.Contains(t.Slug)).ToListAsync();
        db.Technologies.RemoveRange(topik);
        await db.SaveChangesAsync();

        var alat = await db.Tools.Where(t => _alatDibuat.Contains(t.Slug)).ToListAsync();
        db.Tools.RemoveRange(alat);
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Kedelapan endpoint tulis, dijalankan terhadap topik yang sungguh ada.
    /// Slug topiknya disisipkan pemanggil.
    /// </summary>
    private static (HttpMethod Method, string Path, object Body)[] SemuaEndpointTulis(string slug) =>
    [
        (HttpMethod.Post, "/api/v1/technologies",
            new CreateTechnologyRequest("Uji Permukaan Tulis", "Ringkasan untuk uji.", "ai-agents", $"{slug}-lain")),
        (HttpMethod.Post, "/api/v1/tools",
            new CreateToolRequest("Uji Permukaan Tulis", "Ringkasan alat untuk uji.", "https://contoh.test", $"{slug}-alat")),
        (HttpMethod.Put, $"/api/v1/technologies/{slug}/roadmap/prasyarat",
            new RoadmapStepRequest("Prasyarat uji", "Deskripsi prasyarat uji.")),
        (HttpMethod.Post, $"/api/v1/technologies/{slug}/roadmap",
            new RoadmapStepRequest("Langkah uji", "Deskripsi langkah uji.")),
        (HttpMethod.Post, $"/api/v1/technologies/{slug}/tools",
            new AttachToolRequest("alat-apa-saja", null)),
        (HttpMethod.Post, $"/api/v1/technologies/{slug}/projects",
            new ProjectRequest("Proyek uji", "Ringkasan proyek uji.")),
        (HttpMethod.Post, $"/api/v1/technologies/{slug}/resources",
            new ResourceRequest("OfficialDocs", "Sumber uji", "https://contoh.test/spesifikasi")),
        (HttpMethod.Post, $"/api/v1/technologies/{slug}/draf", new { }),
    ];

    private async Task<string> BuatTopikLewatHostTerbukaAsync()
    {
        var slug = $"uji-tulis-{Guid.NewGuid():N}";

        using var host = HostTerbuka();
        using var client = host.CreateClient();

        var response = await client.PostAsJsonAsync(
            new Uri("/api/v1/technologies", UriKind.Relative),
            new CreateTechnologyRequest("Uji Permukaan Tulis", "Ringkasan untuk uji.", "ai-agents", slug));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        _topikDibuat.Add(slug);

        // 🧹 Kedua nama turunan yang dipakai SemuaEndpointTulis didaftarkan DI SINI,
        // bukan di uji yang kebetulan membuatnya. Alasannya ketahuan saat gerbangnya
        // sengaja disabotase untuk dibuktikan merah: begitu gerbangnya jebol, uji
        // yang seharusnya TIDAK membuat apa-apa justru membuat topik - lalu gagal
        // sebelum sempat mendaftarkannya. Justru di hari gerbangnya rusak, basis
        // data pengembang tidak boleh ikut ketularan sampah.
        //
        // Membuang slug yang tidak pernah ada tidak berbiaya apa-apa.
        _topikDibuat.Add($"{slug}-lain");
        _alatDibuat.Add($"{slug}-alat");

        return slug;
    }

    [Fact]
    public async Task Bawaan_TIDAK_memasang_satu_pun_endpoint_tulis()
    {
        var slug = await BuatTopikLewatHostTerbukaAsync();

        using var host = HostBawaan();
        using var client = host.CreateClient();

        foreach (var (method, path, body) in SemuaEndpointTulis(slug))
        {
            using var request = new HttpRequestMessage(method, path) { Content = JsonContent.Create(body) };
            using var response = await client.SendAsync(request);

            // 404 kalau tidak ada rute yang cocok sama sekali; 405 untuk
            // POST /api/v1/technologies, yang alamatnya masih dipegang GET
            // pencarian. Keduanya berarti hal yang sama: rutenya tidak dipasang.
            //
            // Yang TIDAK boleh muncul justru 400 dan 201 - keduanya berarti
            // muatannya sempat dibaca handler.
            Assert.True(
                response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.MethodNotAllowed,
                $"{method} {path} membalas {(int)response.StatusCode}. "
                + "Endpoint tulis seharusnya tidak dipasang sama sekali (ADR-020).");
        }
    }

    [Fact]
    public async Task Bawaan_menutup_MENULIS_tanpa_menutup_MEMBACA()
    {
        // Kendali. Tanpa uji ini, "semuanya 404" tetap hijau seandainya gerbangnya
        // membuang SELURUH rute - termasuk yang menyajikan situsnya.
        var slug = await BuatTopikLewatHostTerbukaAsync();

        using var host = HostBawaan();
        using var client = host.CreateClient();

        var bidang = await client.GetAsync(new Uri("/api/v1/fields", UriKind.Relative));
        Assert.Equal(HttpStatusCode.OK, bidang.StatusCode);
        Assert.Equal(14, (await bidang.Content.ReadFromJsonAsync<IReadOnlyList<FieldResponse>>())?.Count);

        var pencarian = await client.GetAsync(new Uri("/api/v1/technologies", UriKind.Relative));
        Assert.Equal(HttpStatusCode.OK, pencarian.StatusCode);

        // Dan topik yang barusan dibuat tetap bisa dibaca lewat host yang tertutup.
        var topik = await client.GetAsync(new Uri($"/api/v1/technologies/{slug}", UriKind.Relative));
        Assert.Equal(HttpStatusCode.OK, topik.StatusCode);
    }

    [Fact]
    public async Task Sakelar_hidup_membuka_alamat_yang_SAMA_PERSIS()
    {
        // Kendali arah berlawanan, dan ini yang membuat uji pertama berarti:
        // alamat, metode, dan muatan yang sama persis: yang berbeda hanya
        // sakelarnya. Tanpa ini, satu salah ketik di daftar alamat akan
        // meluluskan uji pertama tanpa membuktikan apa pun.
        var slug = await BuatTopikLewatHostTerbukaAsync();

        using var host = HostTerbuka();
        using var client = host.CreateClient();

        foreach (var (method, path, body) in SemuaEndpointTulis(slug))
        {
            using var request = new HttpRequestMessage(method, path) { Content = JsonContent.Create(body) };
            using var response = await client.SendAsync(request);

            // Sebagian memang gagal karena alasan LAIN - menautkan alat yang tak
            // ada 400, naik ke draf sebelum lengkap 400. Yang dijaga di sini
            // hanya satu hal: handlernya SEMPAT DIPANGGIL, jadi jawabannya bukan
            // "rutenya tidak ada".
            Assert.False(
                response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.MethodNotAllowed,
                $"{method} {path} membalas {(int)response.StatusCode} padahal sakelarnya hidup. "
                + "Alamat di daftar uji ini tidak cocok dengan rute yang sebenarnya dipasang.");
        }
    }
}
