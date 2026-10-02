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
/// <c>POST /api/v1/technologies/{slug}/tinjau</c> — jalan sah menuju
/// <c>tinjau</c> (ADR-021), lewat HTTP.
/// </summary>
/// <remarks>
/// 🔑 <b>Ini uji yang membuktikan pencacah resmi RENCANA-V1 akhirnya bisa bergerak.</b>
/// Sampai endpoint ini ada, <c>Technology.MarkReviewed</c> nol pemanggil produksi dan
/// angka "topik berstatus tinjau" adalah pencacah tanpa produsen (#42, ADR-021 §Konteks).
/// Di sini satu topik dibawa dari kosong sampai <c>tinjau</c> lewat HTTP, lalu dibaca
/// ulang untuk membuktikan ia bertahan.
/// <para>
/// 🔴 <b>Nama pemeriksanya diperiksa di DB, bukan di respons.</b> <c>TechnologyResponse</c>
/// membawa <c>ReviewedAt</c>, bukan <c>ReviewedBy</c> (#68 butir 2 — pertanggungjawaban
/// yang belum bisa dibaca klien, sengaja di luar lingkup ADR-021). Jadi "siapa"-nya
/// dibuktikan langsung ke basis data.
/// </para>
/// <para>
/// ⚠️ Menyentuh PostgreSQL sungguhan (<c>run.ps1 up</c> lalu <c>run.ps1 migrate</c>
/// di lokal, service container di CI), dan membersihkan barisnya sendiri — pola yang
/// sama dengan <see cref="ContentSectionEndpointTests"/>.
/// </para>
/// </remarks>
public sealed class TinjauEndpointTests : IAsyncLifetime
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

            // Tanpa baris ini endpoint tulis tidak dipasang (ADR-020). Di produksi
            // jalan ini DITEMPUH workflow bergerbang ADR-021, yang menyalakan sakelar
            // yang sama persis di dalam peti kemas runner, bukan di yang tayang.
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

        var topik = await db.Technologies.Where(t => _topikDibuat.Contains(t.Slug)).ToListAsync();
        db.Technologies.RemoveRange(topik);
        await db.SaveChangesAsync();

        var alat = await db.Tools.Where(t => _alatDibuat.Contains(t.Slug)).ToListAsync();
        db.Tools.RemoveRange(alat);
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task Topik_draf_naik_ke_tinjau_dan_mencatat_pemeriksanya()
    {
        using var host = Host();
        using var client = host.CreateClient();

        var slug = await IsiSampaiDrafAsync(client);

        var response = await client.PostAsJsonAsync(
            new Uri($"/api/v1/technologies/{slug}/tinjau", UriKind.Relative),
            new MarkReviewedRequest("Harasta"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var sesudah = await response.Content.ReadFromJsonAsync<TechnologyResponse>();
        Assert.Equal("HumanReviewed", sesudah!.Maturity);
        Assert.NotNull(sesudah.ReviewedAt);

        // Bertahan: dibaca ulang lewat GET yang sama dengan yang dipakai pengunjung.
        var dibacaUlang = await client.GetFromJsonAsync<TechnologyResponse>(
            new Uri($"/api/v1/technologies/{slug}", UriKind.Relative));
        Assert.Equal("HumanReviewed", dibacaUlang!.Maturity);
        Assert.NotNull(dibacaUlang.ReviewedAt);

        // "Siapa"-nya hanya terbaca di DB (#68 butir 2), dan di sanalah ia dijaga:
        // nama yang dikirim muatan tersimpan apa adanya, di-trim.
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TechnologyDbContext>();
        var tersimpan = await db.Technologies.AsNoTracking().SingleAsync(t => t.Slug == slug);
        Assert.Equal("Harasta", tersimpan.ReviewedBy);
    }

    [Fact]
    public async Task Topik_yang_masih_kurasi_ditolak_400_dan_menyebut_kurasi()
    {
        using var host = Host();
        using var client = host.CreateClient();

        // Baru dibuat = kurasi, isinya belum lengkap. Tidak ada yang bisa diperiksa,
        // jadi naik ke tinjau ditolak — 400, bukan 500, dan bukan 200 yang
        // meluluskan halaman yang belum ada isinya untuk diperiksa.
        var slug = await BuatTopikAsync(client);

        var response = await client.PostAsJsonAsync(
            new Uri($"/api/v1/technologies/{slug}/tinjau", UriKind.Relative),
            new MarkReviewedRequest("Harasta"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("kurasi", await response.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Nama_pemeriksa_kosong_ditolak_400_bermedan_reviewer()
    {
        using var host = Host();
        using var client = host.CreateClient();

        var slug = await IsiSampaiDrafAsync(client);

        var response = await client.PostAsJsonAsync(
            new Uri($"/api/v1/technologies/{slug}/tinjau", UriKind.Relative),
            new MarkReviewedRequest("   "));

        var isi = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        // Medan `reviewer` di dalam `errors` — datang dari ParamName domain, bukan
        // ditulis handler. Membuktikan aturan "tinjau tanpa manusianya mustahil"
        // (ADR-012) sampai ke luar lewat HTTP dengan medan yang benar.
        using var dokumen = JsonDocument.Parse(isi);
        Assert.True(
            dokumen.RootElement.TryGetProperty("errors", out var errors) && errors.TryGetProperty("reviewer", out _),
            $"400-nya harus bermedan 'reviewer'. Badan: {isi}");
    }

    [Fact]
    public async Task Tinjau_pada_topik_yang_tidak_ada_membalas_404()
    {
        using var host = Host();
        using var client = host.CreateClient();

        var response = await client.PostAsJsonAsync(
            new Uri($"/api/v1/technologies/{Unik("tidak-ada")}/tinjau", UriKind.Relative),
            new MarkReviewedRequest("Harasta"));

        // 404: yang tidak ada di ALAMAT, bukan di muatan.
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<string> BuatTopikAsync(HttpClient client)
    {
        var slug = Unik("uji-tinjau");

        var response = await client.PostAsJsonAsync(
            new Uri("/api/v1/technologies", UriKind.Relative),
            new CreateTechnologyRequest("Uji Tinjau", "Ringkasan untuk uji.", "ai-agents", slug));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        _topikDibuat.Add(slug);
        return slug;
    }

    /// <summary>
    /// Topik baru, kelima bagiannya terisi, lalu dinaikkan ke <c>draf</c> — keadaan
    /// yang sah tepat sebelum <c>tinjau</c>. Langkahnya sama dengan
    /// <see cref="ContentSectionEndpointTests"/>, dijalankan lewat HTTP.
    /// </summary>
    private async Task<string> IsiSampaiDrafAsync(HttpClient client)
    {
        var slug = await BuatTopikAsync(client);

        // Overview sudah terisi lewat Summary saat dibuat. Roadmap butuh langkah
        // SESUDAH prasyarat, jadi dua langkah.
        Assert.Equal(HttpStatusCode.OK, (await client.PutAsJsonAsync(
            new Uri($"/api/v1/technologies/{slug}/roadmap/prasyarat", UriKind.Relative),
            new RoadmapStepRequest("Dasar pemrograman", "Bisa menulis fungsi."))).StatusCode);

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

        var draf = await client.PostAsync(new Uri($"/api/v1/technologies/{slug}/draf", UriKind.Relative), null);
        Assert.Equal(HttpStatusCode.OK, draf.StatusCode);
        Assert.Equal("MachineDrafted", (await draf.Content.ReadFromJsonAsync<TechnologyResponse>())!.Maturity);

        return slug;
    }
}
