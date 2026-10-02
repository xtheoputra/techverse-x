using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Hosting;
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

    /// <summary>
    /// Host bersakelar tulis hidup, di lingkungan yang diminta.
    /// </summary>
    /// <remarks>
    /// 🔴 Lingkungannya jadi parameter karena ASP.NET punya DUA jalan untuk permintaan
    /// yang tidak bisa diikat: di <c>Development</c> ia melempar
    /// <c>BadHttpRequestException</c>, di lingkungan lain ia hanya menulis 400.
    /// WebApplicationFactory berjalan di <c>Development</c>, jadi sampai #61 tidak ada
    /// satu uji pun yang pernah melihat jalan yang dipakai produksi.
    /// </remarks>
    private static WebApplicationFactory<Program> HostTerbuka(string lingkungan) =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment(lingkungan);
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
    /// Seluruh endpoint tulis, dijalankan terhadap topik yang sungguh ada.
    /// Slug topiknya disisipkan pemanggil.
    /// </summary>
    /// <remarks>
    /// ⚠️ Sengaja tanpa hitungan. Ringkasan ini dulu menyebut jumlahnya, dan angka
    /// itu basi di endpoint tulis berikutnya. Endpoint tulis baru cukup masuk
    /// DAFTAR ini; uji-uji di bawah langsung menjaganya.
    /// <para>
    /// <c>Body</c> <c>null</c> berarti endpoint itu memang TIDAK menerima badan —
    /// satu-satunya tempat yang dilewati uji muatan cacat. Endpoint berbadan yang
    /// ditulis <c>null</c> di sini akan lolos dari uji itu, jadi <c>null</c> harus
    /// benar, bukan jalan pintas.
    /// </para>
    /// </remarks>
    private static (HttpMethod Method, string Path, object? Body)[] SemuaEndpointTulis(string slug) =>
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
        (HttpMethod.Post, $"/api/v1/technologies/{slug}/draf", null),

        // Tujuannya sengaja tidak pernah ada: dengan sakelar hidup handlernya
        // membaca muatan dan membalas 400 (bukan 404, lihat RequireTopicHandler),
        // dan tidak ada sisi yang tercipta - jadi pembersihan tidak perlu berubah.
        (HttpMethod.Post, $"/api/v1/technologies/{slug}/requires",
            new RequireTopicRequest("topik-yang-tidak-pernah-ada")),

        // Membuang sisi: tujuannya tak pernah ada, jadi dengan sakelar hidup ia
        // tanpa-operasi 200 (DELETE idempoten, beda dari POST yang 400), dengan
        // sakelar mati rutenya tak dipasang - 404. Tanpa badan, jadi ikut dilewati
        // uji muatan cacat #61, sama seperti /draf.
        (HttpMethod.Delete, $"/api/v1/technologies/{slug}/requires/topik-yang-tidak-pernah-ada", null),

        // Topik uji ini baru dibuat, jadi isinya masih `kurasi`: dengan sakelar hidup
        // MarkReviewed menolaknya 400 ("masih kurasi", bukan 404), dengan sakelar mati
        // rutenya tidak dipasang - 404. Keduanya membuktikan gerbang ADR-020, dan tidak
        // ada yang naik ke tinjau, jadi pembersihan tidak perlu berubah. Nama pemeriksa
        // jadi medan teks pertama yang diincar uji muatan cacat #61.
        (HttpMethod.Post, $"/api/v1/technologies/{slug}/tinjau",
            new MarkReviewedRequest("Uji Permukaan Tulis")),
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
            using var request = new HttpRequestMessage(method, path) { Content = Badan(body) };
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
            using var request = new HttpRequestMessage(method, path) { Content = Badan(body) };
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

    /// <summary>
    /// Muatan yang tidak bisa diikat membalas 400 yang MENYEBUT sebabnya — bukan 500
    /// — di setiap endpoint tulis berbadan, di kedua lingkungan (issue #61).
    /// </summary>
    /// <remarks>
    /// 🔴 Kelas cacat #26 yang kembali lewat BENTUK muatan, bukan nilainya. Uji #26
    /// hanya mengirim nilai yang keliru; muatan yang bentuknya keliru tidak pernah
    /// sampai ke validator, dan tidak ada yang mengirimnya.
    /// <para>
    /// 🔑 Kedua lingkungan diuji karena jalannya berbeda (lihat
    /// <see cref="HostTerbuka(string)"/>). Menuntut <c>detail</c> — bukan cuma 400 —
    /// yang membuat baris <c>Production</c> berarti: di sana 400-nya sudah ada, tapi
    /// tanpa satu kata pun tentang sebabnya, jadi pemanggil tidak tahu medan mana
    /// yang keliru.
    /// </para>
    /// </remarks>
    [Theory]
    [InlineData("Development")]
    [InlineData("Production")]
    public async Task Muatan_yang_tidak_bisa_diikat_membalas_400_yang_menyebut_sebabnya(string lingkungan)
    {
        var slug = $"uji-muatan-cacat-{Guid.NewGuid():N}";

        // Tidak ada yang seharusnya tercipta. Kalau ternyata ada, ia tetap dibersihkan -
        // alasan yang sama dengan BuatTopikLewatHostTerbukaAsync.
        _topikDibuat.Add($"{slug}-lain");
        _alatDibuat.Add($"{slug}-alat");

        using var host = HostTerbuka(lingkungan);
        using var client = host.CreateClient();

        var keliru = new List<string>();

        foreach (var (method, path, body) in SemuaEndpointTulis(slug))
        {
            if (body is null)
            {
                continue;
            }

            foreach (var (nama, isi, jejakJson) in MuatanCacat(body))
            {
                using var request = new HttpRequestMessage(method, path)
                {
                    Content = new StringContent(isi, Encoding.UTF8, "application/json"),
                };
                using var response = await client.SendAsync(request);
                var teks = await response.Content.ReadAsStringAsync();
                var detail = DetailMasalah(response, teks);

                if (response.StatusCode != HttpStatusCode.BadRequest
                    || string.IsNullOrWhiteSpace(detail)
                    || (jejakJson is not null && !detail.Contains(jejakJson, StringComparison.Ordinal)))
                {
                    keliru.Add($"{method} {path} [{nama}] -> {(int)response.StatusCode} {teks}");
                }
            }
        }

        Assert.True(
            keliru.Count == 0,
            $"{keliru.Count} jawaban keliru di {lingkungan}:{Environment.NewLine}{string.Join(Environment.NewLine, keliru)}");
    }

    /// <summary>Badan JSON untuk entri <see cref="SemuaEndpointTulis"/>; tanpa badan untuk <c>null</c>.</summary>
    private static JsonContent? Badan(object? body) => body is null ? null : JsonContent.Create(body);

    /// <summary>
    /// Tiga muatan yang TIDAK bisa diikat, diturunkan dari muatan sah sebuah endpoint.
    /// </summary>
    /// <remarks>
    /// Diturunkan, bukan ditulis per endpoint: endpoint berbadan yang masuk
    /// <see cref="SemuaEndpointTulis"/> langsung ikut diuji tanpa satu baris pun di
    /// sini. <c>JejakJson</c> — jalur yang harus disebut balasannya — hanya diisi
    /// untuk muatan bertipe salah, yang letak cacatnya pasti.
    /// </remarks>
    private static IEnumerable<(string Nama, string Isi, string? JejakJson)> MuatanCacat(object sah)
    {
        var json = JsonSerializer.SerializeToNode(sah, JsonSerializerOptions.Web)!.AsObject();

        // Medan teks pertama diganti ANGKA - bentuk persis #61: {"type":0}, {"name":123}.
        var medan = json.First(p => p.Value?.GetValueKind() == JsonValueKind.String).Key;
        var salahTipe = json.DeepClone().AsObject();
        salahTipe[medan] = 12345;
        yield return ("bertipe salah", salahTipe.ToJsonString(), $"$.{medan}");

        // Kurung kurawal penutupnya dibuang.
        yield return ("terpotong", json.ToJsonString()[..^1], null);

        yield return ("kosong", string.Empty, null);
    }

    /// <summary><c>detail</c> sebuah ProblemDetails, atau <c>null</c> kalau balasannya bukan ProblemDetails.</summary>
    private static string? DetailMasalah(HttpResponseMessage response, string teks)
    {
        if (response.Content.Headers.ContentType?.MediaType != "application/problem+json")
        {
            return null;
        }

        using var dokumen = JsonDocument.Parse(teks);
        return dokumen.RootElement.TryGetProperty("detail", out var detail) ? detail.GetString() : null;
    }
}
