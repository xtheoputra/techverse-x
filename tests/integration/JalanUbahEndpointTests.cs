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
/// Jalan UBAH isi yang sudah terpasang, lewat HTTP — ADR-028 Tahap 2, #79:
/// <c>PUT</c> topik, <c>PUT</c> langkah roadmap menurut nomornya, <c>PUT</c> katalog alat.
/// </summary>
/// <remarks>
/// 🔑 <b>Yang dijaga bukan "rutenya ada", melainkan tiga janji.</b> (1) Mengganti teks pada
/// topik <c>tinjau</c> menggugurkannya — dan <b>tersimpan</b>, dibaca ulang lewat GET.
/// (2) Mengulang teks yang sama tidak: PUT yang diulang klien tak boleh membuang kerja
/// pemeriksa. (3) Muatan yang keliru — termasuk teks kepanjangan — membalas 400, bukan 500
/// (kelas cacat #26). Tiap janji diuji bersama kendali arah berlawanannya.
/// <para>
/// ⚠️ Menyentuh PostgreSQL sungguhan (<c>run.ps1 up</c> lalu <c>run.ps1 migrate</c> di
/// lokal, service container di CI), dan membersihkan barisnya sendiri.
/// </para>
/// </remarks>
public sealed class JalanUbahEndpointTests : IAsyncLifetime
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

    // ---- PUT /api/v1/technologies/{slug} -----------------------------------

    [Fact]
    public async Task Put_topik_mengganti_nama_dan_ringkasan_tanpa_menyentuh_slug_dan_bidang()
    {
        using var host = Host();
        using var client = host.CreateClient();
        var (slug, _) = await IsiSampaiDrafAsync(client);

        var response = await client.PutAsJsonAsync(Alamat($"/api/v1/technologies/{slug}"),
            new UpdateTechnologyRequest("Nama Sesudah Diganti", "Ringkasan sesudah diganti."));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var dibaca = await client.GetFromJsonAsync<TechnologyResponse>(Alamat($"/api/v1/technologies/{slug}"));
        Assert.Equal("Nama Sesudah Diganti", dibaca!.Name);
        Assert.Equal("Ringkasan sesudah diganti.", dibaca.Summary);
        Assert.Equal(slug, dibaca.Slug);
        Assert.Equal("ai-agents", dibaca.FieldSlug);

        // Bagian isi lain tak tersentuh.
        Assert.Equal(2, dibaca.Roadmap.Count);
        Assert.Single(dibaca.Tools);
        Assert.Empty(dibaca.MissingSections);
    }

    [Fact]
    public async Task Put_topik_dengan_isi_berbeda_menggugurkan_tinjau_dan_tersimpan()
    {
        using var host = Host();
        using var client = host.CreateClient();
        var (slug, _) = await IsiSampaiTinjauAsync(client);

        var response = await client.PutAsJsonAsync(Alamat($"/api/v1/technologies/{slug}"),
            new UpdateTechnologyRequest("Uji Jalan Ubah", "Ringkasan yang berbeda."));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("MachineDrafted", (await response.Content.ReadFromJsonAsync<TechnologyResponse>())!.Maturity);

        // Bukan hanya di respons: dibaca ulang, dan pemeriksanya terhapus di DB.
        var dibaca = await client.GetFromJsonAsync<TechnologyResponse>(Alamat($"/api/v1/technologies/{slug}"));
        Assert.Equal("MachineDrafted", dibaca!.Maturity);
        Assert.Null(dibaca.ReviewedAt);
        Assert.Null(await PemeriksaDiDbAsync(host, slug));
    }

    [Fact]
    public async Task Put_topik_dengan_isi_yang_SAMA_tidak_menggugurkan_tinjau()
    {
        // Kendali arah berlawanan, dan inilah yang membuat uji di atas berarti. PUT
        // adalah idempoten menurut HTTP; klien yang mengulangnya tidak boleh membuang
        // kerja pemeriksa.
        using var host = Host();
        using var client = host.CreateClient();
        var (slug, _) = await IsiSampaiTinjauAsync(client);

        var response = await client.PutAsJsonAsync(Alamat($"/api/v1/technologies/{slug}"),
            new UpdateTechnologyRequest("  Uji Jalan Ubah ", "Ringkasan untuk uji.\n"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var dibaca = await client.GetFromJsonAsync<TechnologyResponse>(Alamat($"/api/v1/technologies/{slug}"));
        Assert.Equal("HumanReviewed", dibaca!.Maturity);
        Assert.NotNull(dibaca.ReviewedAt);
        Assert.Equal("Harasta", await PemeriksaDiDbAsync(host, slug));
    }

    [Fact]
    public async Task Put_topik_yang_tidak_ada_membalas_404()
    {
        using var host = Host();
        using var client = host.CreateClient();

        var response = await client.PutAsJsonAsync(Alamat($"/api/v1/technologies/{Unik("tidak-ada")}"),
            new UpdateTechnologyRequest("Nama", "Ringkasan."));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData("   ", "Ringkasan.", "name")]
    [InlineData("Nama", "   ", "summary")]
    public async Task Put_topik_dengan_medan_kosong_membalas_400_bermedan_yang_keliru(string nama, string ringkasan, string medan)
    {
        using var host = Host();
        using var client = host.CreateClient();
        var (slug, _) = await IsiSampaiDrafAsync(client);

        var response = await client.PutAsJsonAsync(Alamat($"/api/v1/technologies/{slug}"),
            new UpdateTechnologyRequest(nama, ringkasan));

        await AssertBadRequestBermedanAsync(response, medan);

        // Yang ditolak tak mengubah apa pun.
        var dibaca = await client.GetFromJsonAsync<TechnologyResponse>(Alamat($"/api/v1/technologies/{slug}"));
        Assert.Equal("Uji Jalan Ubah", dibaca!.Name);
        Assert.Equal("Ringkasan untuk uji.", dibaca.Summary);
    }

    [Theory]
    [InlineData(201, 10, "name")]
    [InlineData(10, 2001, "summary")]
    public async Task Put_topik_dengan_teks_kepanjangan_membalas_400_bukan_500(int panjangNama, int panjangRingkasan, string medan)
    {
        using var host = Host();
        using var client = host.CreateClient();
        var (slug, _) = await IsiSampaiDrafAsync(client);

        var response = await client.PutAsJsonAsync(Alamat($"/api/v1/technologies/{slug}"),
            new UpdateTechnologyRequest(new string('n', panjangNama), new string('r', panjangRingkasan)));

        await AssertBadRequestBermedanAsync(response, medan);
    }

    // ---- PUT /api/v1/technologies/{slug}/roadmap/{order} -------------------

    [Fact]
    public async Task Put_langkah_roadmap_mengganti_langkah_yang_ada_tanpa_mengubah_jumlah_dan_urutan()
    {
        using var host = Host();
        using var client = host.CreateClient();
        var (slug, _) = await IsiSampaiDrafAsync(client);

        var response = await client.PutAsJsonAsync(Alamat($"/api/v1/technologies/{slug}/roadmap/1"),
            new RoadmapStepRequest("Langkah pertama, diperbaiki", "Uraian yang sudah diperbaiki."));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var dibaca = await client.GetFromJsonAsync<TechnologyResponse>(Alamat($"/api/v1/technologies/{slug}"));
        Assert.Equal([0, 1], dibaca!.Roadmap.Select(l => l.Order));
        Assert.Equal("Dasar pemrograman", dibaca.Roadmap[0].Title);
        Assert.Equal("Langkah pertama, diperbaiki", dibaca.Roadmap[1].Title);
        Assert.Equal("Uraian yang sudah diperbaiki.", dibaca.Roadmap[1].Description);
    }

    [Fact]
    public async Task Put_langkah_nol_sama_dengan_prasyarat()
    {
        using var host = Host();
        using var client = host.CreateClient();
        var (slug, _) = await IsiSampaiDrafAsync(client);

        var response = await client.PutAsJsonAsync(Alamat($"/api/v1/technologies/{slug}/roadmap/0"),
            new RoadmapStepRequest("Prasyarat lewat nomor", "Diganti lewat nomor nol."));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var dibaca = await client.GetFromJsonAsync<TechnologyResponse>(Alamat($"/api/v1/technologies/{slug}"));
        Assert.Equal(2, dibaca!.Roadmap.Count);
        Assert.True(dibaca.Roadmap[0].IsPrerequisite);
        Assert.Equal("Prasyarat lewat nomor", dibaca.Roadmap[0].Title);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(99)]
    [InlineData(-1)]
    public async Task Put_langkah_dengan_nomor_yang_belum_ada_membalas_400_dan_tidak_membuat_langkah(int nomor)
    {
        // Nomor adalah ALAMAT, bukan isian. Kalau pintu ini diam-diam membuat langkah,
        // roadmap berlubang (0, 1, 99) kembali mungkin — persis yang dicegah ADR-012.
        using var host = Host();
        using var client = host.CreateClient();
        var (slug, _) = await IsiSampaiDrafAsync(client);

        var response = await client.PutAsJsonAsync(Alamat($"/api/v1/technologies/{slug}/roadmap/{nomor}"),
            new RoadmapStepRequest("Judul", "Uraian"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("0..1", await response.Content.ReadAsStringAsync(), StringComparison.Ordinal);

        var dibaca = await client.GetFromJsonAsync<TechnologyResponse>(Alamat($"/api/v1/technologies/{slug}"));
        Assert.Equal([0, 1], dibaca!.Roadmap.Select(l => l.Order));
    }

    [Fact]
    public async Task Put_langkah_pada_topik_yang_tidak_ada_membalas_404()
    {
        using var host = Host();
        using var client = host.CreateClient();

        var response = await client.PutAsJsonAsync(Alamat($"/api/v1/technologies/{Unik("tidak-ada")}/roadmap/1"),
            new RoadmapStepRequest("Judul", "Uraian"));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Put_langkah_dengan_teks_kepanjangan_membalas_400_bukan_500()
    {
        using var host = Host();
        using var client = host.CreateClient();
        var (slug, _) = await IsiSampaiDrafAsync(client);

        await AssertBadRequestBermedanAsync(
            await client.PutAsJsonAsync(Alamat($"/api/v1/technologies/{slug}/roadmap/1"),
                new RoadmapStepRequest(new string('j', 201), "Uraian")),
            "title");

        await AssertBadRequestBermedanAsync(
            await client.PutAsJsonAsync(Alamat($"/api/v1/technologies/{slug}/roadmap/1"),
                new RoadmapStepRequest("Judul", new string('u', 2001))),
            "description");

        // Jalan TAMBAH dan prasyarat ikut terjaga: dulu hanya kolom basis data yang
        // menolak teks sepanjang itu, sebagai 500.
        await AssertBadRequestBermedanAsync(
            await client.PostAsJsonAsync(Alamat($"/api/v1/technologies/{slug}/roadmap"),
                new RoadmapStepRequest("Judul", new string('u', 2001))),
            "description");

        await AssertBadRequestBermedanAsync(
            await client.PutAsJsonAsync(Alamat($"/api/v1/technologies/{slug}/roadmap/prasyarat"),
                new RoadmapStepRequest(new string('j', 201), "Uraian")),
            "title");
    }

    [Fact]
    public async Task Put_langkah_dengan_teks_berbeda_menggugurkan_tinjau_dan_teks_sama_tidak()
    {
        using var host = Host();
        using var client = host.CreateClient();
        var (slug, _) = await IsiSampaiTinjauAsync(client);

        // Teks yang SAMA persis (langkah 1 dari IsiSampaiDrafAsync): tetap tinjau.
        var sama = await client.PutAsJsonAsync(Alamat($"/api/v1/technologies/{slug}/roadmap/1"),
            new RoadmapStepRequest("Langkah pertama", "Menjalankan contoh."));
        Assert.Equal(HttpStatusCode.OK, sama.StatusCode);
        Assert.Equal("HumanReviewed", (await sama.Content.ReadFromJsonAsync<TechnologyResponse>())!.Maturity);

        // Teks berbeda: gugur, dan tersimpan.
        var beda = await client.PutAsJsonAsync(Alamat($"/api/v1/technologies/{slug}/roadmap/1"),
            new RoadmapStepRequest("Langkah pertama", "Uraian yang kini berbeda."));
        Assert.Equal(HttpStatusCode.OK, beda.StatusCode);

        var dibaca = await client.GetFromJsonAsync<TechnologyResponse>(Alamat($"/api/v1/technologies/{slug}"));
        Assert.Equal("MachineDrafted", dibaca!.Maturity);
        Assert.Null(await PemeriksaDiDbAsync(host, slug));
    }

    // ---- PUT /api/v1/tools/{slug} ------------------------------------------

    [Fact]
    public async Task Put_alat_mengganti_katalog_dan_terbaca_di_topik_yang_menautkannya()
    {
        using var host = Host();
        using var client = host.CreateClient();
        var (slug, alat) = await IsiSampaiDrafAsync(client);

        var response = await client.PutAsJsonAsync(Alamat($"/api/v1/tools/{alat}"),
            new UpdateToolRequest("Alat Uji Diganti", "Ringkasan alat yang diganti.", "https://example.com/baru"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var diganti = await response.Content.ReadFromJsonAsync<ToolResponse>();
        Assert.Equal(alat, diganti!.Slug);
        Assert.Equal("Alat Uji Diganti", diganti.Name);
        Assert.Equal("https://example.com/baru", diganti.Homepage);

        var topik = await client.GetFromJsonAsync<TechnologyResponse>(Alamat($"/api/v1/technologies/{slug}"));
        var tertaut = Assert.Single(topik!.Tools);
        Assert.Equal("Alat Uji Diganti", tertaut.Name);
        Assert.Equal("Ringkasan alat yang diganti.", tertaut.Summary);

        // Catatan milik TAUTAN, bukan katalog: tak ikut berubah.
        Assert.Equal("Dipakai di langkah pertama.", tertaut.Note);
    }

    [Fact]
    public async Task Put_alat_yang_tidak_ada_membalas_404()
    {
        using var host = Host();
        using var client = host.CreateClient();

        var response = await client.PutAsJsonAsync(Alamat($"/api/v1/tools/{Unik("tidak-ada")}"),
            new UpdateToolRequest("Nama", "Ringkasan."));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData("   ", "Ringkasan.", "name")]
    [InlineData("Nama", "Ringkasan.", "homepage")]
    public async Task Put_alat_dengan_muatan_keliru_membalas_400_bermedan_yang_keliru(string nama, string ringkasan, string medan)
    {
        using var host = Host();
        using var client = host.CreateClient();
        var (_, alat) = await IsiSampaiDrafAsync(client);

        var beranda = medan == "homepage" ? new string('h', 501) : null;
        var response = await client.PutAsJsonAsync(Alamat($"/api/v1/tools/{alat}"),
            new UpdateToolRequest(nama, ringkasan, beranda));

        await AssertBadRequestBermedanAsync(response, medan);
    }

    [Fact]
    public async Task Put_alat_dengan_teks_kepanjangan_membalas_400_bukan_500()
    {
        using var host = Host();
        using var client = host.CreateClient();
        var (_, alat) = await IsiSampaiDrafAsync(client);

        await AssertBadRequestBermedanAsync(
            await client.PutAsJsonAsync(Alamat($"/api/v1/tools/{alat}"),
                new UpdateToolRequest(new string('n', 201), "Ringkasan.")),
            "name");

        await AssertBadRequestBermedanAsync(
            await client.PutAsJsonAsync(Alamat($"/api/v1/tools/{alat}"),
                new UpdateToolRequest("Nama", new string('r', 2001))),
            "summary");
    }

    [Fact]
    public async Task Put_alat_bersama_menggugurkan_tinjau_HANYA_topik_yang_menautkannya()
    {
        // Inti jalan ini: alat dipakai bersama (ADR-015), jadi ringkasannya tampil di
        // setiap halaman yang menautkannya. Tiga topik: A (tinjau, menautkan alat),
        // B (draf, menautkan alat), C (tinjau, TIDAK menautkan alat). Yang gugur hanya A.
        using var host = Host();
        using var client = host.CreateClient();

        var (topikA, alat) = await IsiSampaiTinjauAsync(client);
        var (topikB, _) = await IsiSampaiDrafAsync(client, alat);
        var (topikC, _) = await IsiSampaiTinjauAsync(client);

        var response = await client.PutAsJsonAsync(Alamat($"/api/v1/tools/{alat}"),
            new UpdateToolRequest("Alat Uji", "Ringkasan alat yang berubah isinya.", "https://example.com"));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var a = await client.GetFromJsonAsync<TechnologyResponse>(Alamat($"/api/v1/technologies/{topikA}"));
        var b = await client.GetFromJsonAsync<TechnologyResponse>(Alamat($"/api/v1/technologies/{topikB}"));
        var c = await client.GetFromJsonAsync<TechnologyResponse>(Alamat($"/api/v1/technologies/{topikC}"));

        Assert.Equal("MachineDrafted", a!.Maturity);
        Assert.Null(a.ReviewedAt);
        Assert.Null(await PemeriksaDiDbAsync(host, topikA));

        Assert.Equal("MachineDrafted", b!.Maturity);

        Assert.Equal("HumanReviewed", c!.Maturity);
        Assert.NotNull(c.ReviewedAt);
        Assert.Equal("Harasta", await PemeriksaDiDbAsync(host, topikC));
    }

    [Fact]
    public async Task Put_alat_yang_hanya_ganti_beranda_atau_mengulang_teks_tidak_menggugurkan_apa_pun()
    {
        using var host = Host();
        using var client = host.CreateClient();
        var (slug, alat) = await IsiSampaiTinjauAsync(client);

        // Hanya beranda berbeda: tautan, bukan teks (ADR-012: memperbaiki tautan tidak
        // boleh membuang kerja pemeriksa).
        Assert.Equal(HttpStatusCode.OK, (await client.PutAsJsonAsync(Alamat($"/api/v1/tools/{alat}"),
            new UpdateToolRequest("Alat Uji", "Dipakai uji integrasi.", "https://example.com/pindah"))).StatusCode);

        // Teks sama sesudah dipangkas.
        Assert.Equal(HttpStatusCode.OK, (await client.PutAsJsonAsync(Alamat($"/api/v1/tools/{alat}"),
            new UpdateToolRequest("  Alat Uji ", "Dipakai uji integrasi.\n", "https://example.com/pindah"))).StatusCode);

        var dibaca = await client.GetFromJsonAsync<TechnologyResponse>(Alamat($"/api/v1/technologies/{slug}"));
        Assert.Equal("HumanReviewed", dibaca!.Maturity);
        Assert.Equal("Harasta", await PemeriksaDiDbAsync(host, slug));
        Assert.Equal("https://example.com/pindah", Assert.Single(dibaca.Tools).Homepage);
    }

    // ---- Alat bantu ---------------------------------------------------------

    private static async Task AssertBadRequestBermedanAsync(HttpResponseMessage response, string medan)
    {
        var isi = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest, $"harus 400, bukan {(int)response.StatusCode}. Badan: {isi}");

        using var dokumen = JsonDocument.Parse(isi);
        Assert.True(
            dokumen.RootElement.TryGetProperty("errors", out var errors) && errors.TryGetProperty(medan, out _),
            $"400-nya harus bermedan '{medan}'. Badan: {isi}");
    }

    private static async Task<string?> PemeriksaDiDbAsync(WebApplicationFactory<Program> host, string slug)
    {
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TechnologyDbContext>();
        return (await db.Technologies.AsNoTracking().SingleAsync(t => t.Slug == slug)).ReviewedBy;
    }

    private async Task<string> BuatTopikAsync(HttpClient client)
    {
        var slug = Unik("uji-ubah");

        var response = await client.PostAsJsonAsync(
            Alamat("/api/v1/technologies"),
            new CreateTechnologyRequest("Uji Jalan Ubah", "Ringkasan untuk uji.", "ai-agents", slug));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        _topikDibuat.Add(slug);
        return slug;
    }

    /// <summary>
    /// Topik baru, kelima bagiannya terisi, lalu <c>draf</c>. Alatnya dibuat baru kecuali
    /// <paramref name="alatBersama"/> diberikan — supaya satu alat bisa dipakai beberapa topik.
    /// </summary>
    private async Task<(string Slug, string Alat)> IsiSampaiDrafAsync(HttpClient client, string? alatBersama = null)
    {
        var slug = await BuatTopikAsync(client);

        Assert.Equal(HttpStatusCode.OK, (await client.PutAsJsonAsync(
            Alamat($"/api/v1/technologies/{slug}/roadmap/prasyarat"),
            new RoadmapStepRequest("Dasar pemrograman", "Bisa menulis fungsi."))).StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync(
            Alamat($"/api/v1/technologies/{slug}/roadmap"),
            new RoadmapStepRequest("Langkah pertama", "Menjalankan contoh."))).StatusCode);

        var alat = alatBersama;
        if (alat is null)
        {
            alat = Unik("alat");
            _alatDibuat.Add(alat);
            Assert.Equal(HttpStatusCode.Created, (await client.PostAsJsonAsync(
                Alamat("/api/v1/tools"),
                new CreateToolRequest("Alat Uji", "Dipakai uji integrasi.", "https://example.com", alat))).StatusCode);
        }

        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync(
            Alamat($"/api/v1/technologies/{slug}/tools"),
            new AttachToolRequest(alat, "Dipakai di langkah pertama."))).StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync(
            Alamat($"/api/v1/technologies/{slug}/projects"),
            new ProjectRequest("Proyek kecil", "Membangun contoh minimal."))).StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync(
            Alamat($"/api/v1/technologies/{slug}/resources"),
            new ResourceRequest("OfficialDocs", "Dokumentasi resmi", "https://example.com/docs"))).StatusCode);

        var draf = await client.PostAsync(Alamat($"/api/v1/technologies/{slug}/draf"), null);
        Assert.Equal(HttpStatusCode.OK, draf.StatusCode);

        return (slug, alat);
    }

    private async Task<(string Slug, string Alat)> IsiSampaiTinjauAsync(HttpClient client)
    {
        var hasil = await IsiSampaiDrafAsync(client);

        var tinjau = await client.PostAsJsonAsync(
            Alamat($"/api/v1/technologies/{hasil.Slug}/tinjau"),
            new MarkReviewedRequest("Harasta"));
        Assert.Equal(HttpStatusCode.OK, tinjau.StatusCode);

        return hasil;
    }
}
