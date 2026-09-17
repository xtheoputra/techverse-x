using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TechVerseX.Contracts.Technology;
using TechVerseX.TechnologyService.Infrastructure.Persistence;

namespace TechVerseX.Api.Tests;

/// <summary>
/// Relasi antar-topik lewat HTTP — <c>POST /api/v1/technologies/{slug}/requires</c>
/// sampai ke kedua halaman yang membacanya (ADR-023).
/// </summary>
/// <remarks>
/// 🔑 <b>Setiap topik dibuat di permintaannya SENDIRI, lebih dulu.</b> Sisi selalu
/// ditambahkan ke agregat yang sudah tersimpan, karena hanya di keadaan itu kunci
/// sisi yang dibuat domain berubah jadi UPDATE kalau <c>ValueGeneratedNever</c>
/// hilang — 500 di permintaan tulis pertama. Membuat topik dan sisinya dalam satu
/// <c>SaveChanges</c> akan meluluskan uji ini persis di hari penjaganya rusak.
/// <para>
/// Yang dijaga di sini penjaga LAPIS APLIKASI: idempotensi (Include + return awal
/// domain), aturan dua topik tidak boleh saling mensyaratkan, 400 yang bisa
/// dibedakan untuk tiap tujuan yang keliru, dan pemuat relasi yang sama di jalan
/// baca maupun tulis. Penjaga basis datanya ada di
/// <see cref="PrasyaratTopikPersistenceTests"/>.
/// </para>
/// <para>
/// ⚠️ <b>Urutan pembersihan penting.</b> Ujung tujuan sisi Restrict, jadi
/// <see cref="DisposeAsync"/> membuang sisi (kedua arah) SEBELUM topiknya. Kalau
/// tidak, pembersihannya sendiri ditolak basis data dan sampahnya tertinggal di
/// basis data pengembang.
/// </para>
/// <para>
/// ⚠️ Menyentuh PostgreSQL sungguhan (<c>run.ps1 up</c> lalu <c>run.ps1 migrate</c>
/// di lokal, service container di CI).
/// </para>
/// </remarks>
public sealed class PrasyaratTopikTests : IAsyncLifetime
{
    private const string Postgres =
        "Host=localhost;Port=5432;Database=techversex;Username=techversex;Password=techversex_dev";

    private readonly List<string> _topikDibuat = [];

    /// <summary>Untuk MENULIS: endpoint tulis tidak dipasang tanpa sakelar ini (ADR-020).</summary>
    private static WebApplicationFactory<Program> HostTerbuka() =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:Postgres", Postgres);
            builder.UseSetting("ConnectionStrings:Redis", string.Empty);
            builder.UseSetting("Editorial:WritesEnabled", "true");
        });

    /// <summary>
    /// Untuk MEMBACA, dalam bentuk produksi: relasi harus tampil dari host yang
    /// tidak pernah bisa menulisnya.
    /// </summary>
    private static WebApplicationFactory<Program> HostBawaan() =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:Postgres", Postgres);
            builder.UseSetting("ConnectionStrings:Redis", string.Empty);
        });

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        if (_topikDibuat.Count == 0)
        {
            return;
        }

        using var host = HostBawaan();
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TechnologyDbContext>();

        // Sisi DULU, kedua arah: ujung tujuan Restrict. Interpolasi, bukan
        // ExecuteSqlRawAsync(sql, larik) - lihat PrasyaratTopikPersistenceTests
        // soal larik yang terikat sebagai daftar parameter.
        var slugs = _topikDibuat.ToArray();
        await db.Database.ExecuteSqlAsync(
            $"""
            DELETE FROM technology.technology_relationships r
            USING technology.technologies t
            WHERE t."Slug" = ANY({slugs})
              AND (r."FromTechnologyId" = t."Id" OR r."ToTechnologyId" = t."Id");
            """);

        var topik = await db.Technologies.Where(t => _topikDibuat.Contains(t.Slug)).ToListAsync();
        db.Technologies.RemoveRange(topik);
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task Sisi_ke_topik_yang_SUDAH_tersimpan_membalas_200_bukan_500()
    {
        using var host = HostTerbuka();
        using var client = host.CreateClient();

        var a = await BuatTopikAsync(client, "agen", "ai-agents");
        var b = await BuatTopikAsync(client, "fondasi", "ai-machine-learning");

        var sebelum = await client.GetFromJsonAsync<TechnologyResponse>(new Uri($"/api/v1/technologies/{a}", UriKind.Relative));

        var response = await RequireAsync(client, a, b);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var topikA = await response.Content.ReadFromJsonAsync<TechnologyResponse>();
        var syarat = Assert.Single(topikA!.Requires);
        Assert.Equal(b, syarat.Slug);

        // Label kematangan dan bidang tujuan, bukan milik topik asal: judul yang
        // ditautkan tidak boleh bepergian tanpa labelnya sendiri (ADR-012).
        Assert.Equal("Curated", syarat.Maturity);
        Assert.Equal("ai-machine-learning", syarat.FieldSlug);
        Assert.Equal("AI & Machine Learning", syarat.FieldName);
        Assert.Empty(topikA.RequiredBy);

        // Relasi bukan bagian template (ADR-023): menambah sisi tidak mengubah
        // daftar bagian yang dianggap kosong, ke arah mana pun.
        Assert.Equal(sebelum!.MissingSections, topikA.MissingSections);
    }

    [Fact]
    public async Task Sisi_disimpan_sekali_dan_tampil_di_kedua_halaman()
    {
        string a, b;

        using (var terbuka = HostTerbuka())
        using (var tulis = terbuka.CreateClient())
        {
            a = await BuatTopikAsync(tulis, "butuh", "ai-agents");
            b = await BuatTopikAsync(tulis, "dibutuhkan", "ai-machine-learning");
            Assert.Equal(HttpStatusCode.OK, (await RequireAsync(tulis, a, b)).StatusCode);
        }

        using var host = HostBawaan();
        using var client = host.CreateClient();

        var halamanA = await client.GetFromJsonAsync<TechnologyResponse>(new Uri($"/api/v1/technologies/{a}", UriKind.Relative));
        Assert.Equal(b, Assert.Single(halamanA!.Requires).Slug);
        Assert.Empty(halamanA.RequiredBy);

        // 🔑 Arah kebalikan DITURUNKAN saat dibaca: halaman B menampilkan A
        // walau tidak ada satu baris pun milik agregat B.
        var halamanB = await client.GetFromJsonAsync<TechnologyResponse>(new Uri($"/api/v1/technologies/{b}", UriKind.Relative));
        var pembutuh = Assert.Single(halamanB!.RequiredBy);
        Assert.Equal(a, pembutuh.Slug);
        Assert.Equal("ai-agents", pembutuh.FieldSlug);
        Assert.Equal("Curated", pembutuh.Maturity);
        Assert.Empty(halamanB.Requires);

        Assert.Equal(1, await HitungSisiAsync(a, b));
    }

    [Fact]
    public async Task Mengulang_permintaan_yang_sama_200_dan_tetap_satu_baris()
    {
        using var host = HostTerbuka();
        using var client = host.CreateClient();

        var a = await BuatTopikAsync(client, "ulang-asal", "ai-agents");
        var b = await BuatTopikAsync(client, "ulang-tujuan", "ai-machine-learning");

        Assert.Equal(HttpStatusCode.OK, (await RequireAsync(client, a, b)).StatusCode);

        var ulang = await RequireAsync(client, a, b);

        Assert.Equal(HttpStatusCode.OK, ulang.StatusCode);
        Assert.Single((await ulang.Content.ReadFromJsonAsync<TechnologyResponse>())!.Requires);
        Assert.Equal(1, await HitungSisiAsync(a, b));
    }

    [Fact]
    public async Task Arah_berlawanan_ditolak_400_dan_tidak_menambah_baris()
    {
        using var host = HostTerbuka();
        using var client = host.CreateClient();

        var a = await BuatTopikAsync(client, "maju", "ai-agents");
        var b = await BuatTopikAsync(client, "balik", "ai-machine-learning");

        Assert.Equal(HttpStatusCode.OK, (await RequireAsync(client, a, b)).StatusCode);

        var balik = await RequireAsync(client, b, a);
        var isi = await balik.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, balik.StatusCode);
        Assert.Contains("saling mensyaratkan", isi, StringComparison.Ordinal);
        Assert.Equal(1, await HitungSisiAsync(a, b));
    }

    /// <summary>
    /// Keempat tujuan yang keliru membalas 400 — dan masing-masing dengan pesan
    /// yang menunjuk perbaikannya sendiri. Semuanya 400, jadi pesannya yang harus
    /// membedakan.
    /// </summary>
    /// <param name="tujuan">
    /// Slug tujuan. <c>(diri)</c> diganti slug topik asal itu sendiri.
    /// </param>
    /// <param name="pesan">Potongan pesan yang wajib ada di badan balasan.</param>
    [Theory]
    [InlineData("topik-yang-tidak-pernah-ada", "tidak ada")]
    [InlineData("ai-agents", "bidang")]
    [InlineData("!!!", "bukan slug topik yang sah")]
    [InlineData("(diri)", "dirinya sendiri")]
    public async Task Target_keliru_membalas_400_bukan_500(string tujuan, string pesan)
    {
        using var host = HostTerbuka();
        using var client = host.CreateClient();

        var a = await BuatTopikAsync(client, "keliru", "ai-agents");

        var response = await RequireAsync(client, a, tujuan == "(diri)" ? a : tujuan);
        var isi = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(pesan, isi, StringComparison.Ordinal);

        // Nama medannya medan MUATAN, bukan nama parameter domain.
        Assert.Contains("\"topicSlug\"", isi, StringComparison.Ordinal);
        Assert.Equal(0, await HitungSisiAsync(a, a));
    }

    [Fact]
    public async Task Sumber_tak_ada_membalas_404()
    {
        using var host = HostTerbuka();
        using var client = host.CreateClient();

        // Tujuannya SUNGGUH ada, supaya yang tidak ditemukan pasti topik di alamat
        // - bukan tujuan di muatan, yang jawabannya 400.
        var b = await BuatTopikAsync(client, "tujuan-nyata", "ai-machine-learning");

        var response = await RequireAsync(client, $"uji-syarat-tak-ada-{Guid.NewGuid():N}", b);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(0, await HitungSisiAsync(b, b));
    }

    [Fact]
    public async Task Menambah_bagian_ke_topik_bersisi_tetap_mengembalikan_sisinya()
    {
        using var host = HostTerbuka();
        using var client = host.CreateClient();

        var a = await BuatTopikAsync(client, "bersisi", "ai-agents");
        var b = await BuatTopikAsync(client, "disisi", "ai-machine-learning");

        Assert.Equal(HttpStatusCode.OK, (await RequireAsync(client, a, b)).StatusCode);

        // Endpoint bagian isi TIDAK tahu apa-apa soal relasi, tapi responsnya
        // bentuk lengkap topik yang sama. Kalau jalur tulis memuat relasi dengan
        // cara lain dari GET - atau lupa memuatnya - respons ini diam-diam
        // melaporkan topik tanpa prasyarat.
        var response = await client.PutAsJsonAsync(
            new Uri($"/api/v1/technologies/{a}/roadmap/prasyarat", UriKind.Relative),
            new RoadmapStepRequest("Dasar pemrograman", "Bisa menulis fungsi."));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var topik = await response.Content.ReadFromJsonAsync<TechnologyResponse>();
        Assert.Equal(b, Assert.Single(topik!.Requires).Slug);
    }

    /// <summary>
    /// Membuat satu topik dalam permintaannya sendiri. Slug didaftarkan untuk
    /// dibersihkan SEBELUM asersi apa pun: kalau pembuatannya yang gagal, membuang
    /// slug yang tidak pernah ada tidak berbiaya apa-apa.
    /// </summary>
    private async Task<string> BuatTopikAsync(HttpClient client, string peran, string bidang)
    {
        var slug = $"uji-syarat-{peran}-{Guid.NewGuid():N}";
        _topikDibuat.Add(slug);

        var response = await client.PostAsJsonAsync(
            new Uri("/api/v1/technologies", UriKind.Relative),
            new CreateTechnologyRequest($"Uji Syarat {peran}", "Ringkasan untuk uji.", bidang, slug));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return slug;
    }

    private static Task<HttpResponseMessage> RequireAsync(HttpClient client, string sumber, string tujuan) =>
        client.PostAsJsonAsync(
            new Uri($"/api/v1/technologies/{sumber}/requires", UriKind.Relative),
            new RequireTopicRequest(tujuan));

    /// <summary>
    /// Baris yang menyentuh salah satu dari dua topik, arah mana pun — dihitung
    /// lewat SQL mentah supaya satu baris kebalikan yang diam-diam tersimpan tidak
    /// bersembunyi di balik pemuat relasi.
    /// </summary>
    private static async Task<int> HitungSisiAsync(string slugA, string slugB)
    {
        using var host = HostBawaan();
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TechnologyDbContext>();

        var pasangan = new[] { slugA, slugB };

        return await db.Database
            .SqlQuery<int>(
                $"""
                SELECT count(*)::int AS "Value"
                FROM technology.technology_relationships r
                WHERE r."FromTechnologyId" IN (SELECT "Id" FROM technology.technologies WHERE "Slug" = ANY({pasangan}))
                   OR r."ToTechnologyId" IN (SELECT "Id" FROM technology.technologies WHERE "Slug" = ANY({pasangan}))
                """)
            .SingleAsync();
    }
}
