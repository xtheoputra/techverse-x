using System.Data.Common;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TechVerseX.Contracts.Technology;
using TechVerseX.TechnologyService.Domain;
using TechVerseX.TechnologyService.Features.SearchTechnology;
using TechVerseX.TechnologyService.Infrastructure.Persistence;

namespace TechVerseX.Api.Tests;

/// <summary>
/// Pencarian teks penuh PostgreSQL — sasaran Bulan 3 di <c>docs/RENCANA-V1.md</c>,
/// diputuskan di <c>ADR-022</c>.
/// </summary>
/// <remarks>
/// 🔑 <b>Berkas ini WAJIB uji integrasi, bukan uji unit, dan alasannya lebih dari
/// "butuh basis data".</b> Yang diuji di sini hampir seluruhnya perilaku
/// PostgreSQL: kolom <c>GENERATED ALWAYS ... STORED</c> yang mengisi dirinya
/// sendiri, kamus <c>english</c> yang memotong <c>agents</c> jadi <c>agent</c>,
/// bobot A/B yang menentukan urutan, dan <c>websearch_to_tsquery</c> yang tidak
/// melempar pada masukan yang <c>to_tsquery</c> tolak. Tidak satu pun dari itu
/// ada di kode C# — jadi uji yang mengganti PostgreSQL dengan penyedia dalam
/// memori akan hijau tanpa menyentuh apa pun yang sebenarnya bekerja.
/// <para>
/// 🔑 <b>Hampir setiap uji di sini membawa KENDALI yang dijalankan lewat SQL
/// mentah.</b> Tanpa kendali itu, "ketemu" tidak membuktikan apa pun: ia sama
/// bunyinya dengan hasil yang sudah bisa ditemukan <c>ILIKE</c> sebelum perubahan
/// ini. Yang dibuktikan kendali-kendali itu adalah bahwa jalur lama <b>tidak
/// bisa</b> menemukannya.
/// </para>
/// <para>
/// ⚠️ Kata uji sengaja bukan kata yang ada di dunia nyata (<c>zarqun</c>,
/// <c>pelantur</c>) supaya hasilnya tidak bisa tercampur isi sungguhan — baik
/// yang disemai migrasi maupun yang ditinggalkan <c>run.ps1 seed</c> di mesin
/// pengembang.
/// </para>
/// <para>
/// ⚠️ Menyentuh PostgreSQL sungguhan (<c>run.ps1 up</c> lalu <c>run.ps1 migrate</c>
/// di lokal, service container di CI).
/// </para>
/// </remarks>
public sealed class PencarianTests : IAsyncLifetime
{
    private const string Postgres =
        "Host=localhost;Port=5432;Database=techversex;Username=techversex;Password=techversex_dev";

    private readonly List<string> _topikDibuat = [];

    private static WebApplicationFactory<Program> Host() =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:Postgres", Postgres);
            builder.UseSetting("ConnectionStrings:Redis", string.Empty);

            // Pencarian permukaan BACA — ia hidup tanpa baris ini. Yang butuh
            // baris ini cuma pembuatan topik contohnya (ADR-020).
            builder.UseSetting("Editorial:WritesEnabled", "true");
        });

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        if (_topikDibuat.Count == 0)
        {
            return;
        }

        using var host = Host();
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TechnologyDbContext>();

        var topik = await db.Technologies.Where(t => _topikDibuat.Contains(t.Slug)).ToListAsync();
        db.Technologies.RemoveRange(topik);
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Membuat satu topik lewat agregat, BUKAN lewat HTTP.
    /// </summary>
    /// <remarks>
    /// Slug-nya didaftarkan untuk dibersihkan <b>di baris tempat barisnya
    /// dibuat</b>, bukan sesudah pemeriksaan berhasil. Pelajaran Sesi 11: uji
    /// yang bersih-bersih hanya di jalur suksesnya sama saja dengan tidak
    /// bersih-bersih, dan justru di hari sesuatu rusak basis data pengembang
    /// paling mudah kotor.
    /// </remarks>
    private async Task<string> BuatTopikAsync(
        WebApplicationFactory<Program> host,
        string nama,
        string ringkasan,
        string fieldSlug = "ai-agents")
    {
        var slug = $"uji-cari-{Guid.CreateVersion7():N}";
        _topikDibuat.Add(slug);

        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TechnologyDbContext>();

        var field = await db.Fields.SingleAsync(f => f.Slug == fieldSlug);
        db.Technologies.Add(Technology.Create(nama, ringkasan, field.Id, slug));
        await db.SaveChangesAsync();

        return slug;
    }

    private static async Task<SearchResponse> CariAsync(HttpClient client, string q)
    {
        var response = await client.GetAsync(
            new Uri($"/api/v1/search?q={Uri.EscapeDataString(q)}", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var hasil = await response.Content.ReadFromJsonAsync<SearchResponse>();
        Assert.NotNull(hasil);
        return hasil!;
    }

    private static async Task<int> HitungSqlAsync(WebApplicationFactory<Program> host, string sql, params object[] args)
    {
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TechnologyDbContext>();
        return await db.Database.SqlQueryRaw<int>(sql, args).SingleAsync();
    }

    [Fact]
    public async Task Urutan_kata_dibalik_tetap_ketemu()
    {
        using var host = Host();
        using var client = host.CreateClient();

        var slug = await BuatTopikAsync(host, "Zarqun Pelantur", "Catatan uji untuk pencarian.");

        var hasil = await CariAsync(client, "pelantur zarqun");

        Assert.Contains(hasil.Technologies.Items, t => t.Slug == slug);

        // 🔑 KENDALI. Tanpa baris ini uji di atas tidak membuktikan apa-apa:
        // ILIKE juga akan menemukannya kalau potongan hurufnya kebetulan ada.
        // Di sini ia TIDAK ada — "pelantur zarqun" bukan substring dari
        // "Zarqun Pelantur" — jadi jalur lama memang tidak sanggup, dan yang
        // menemukannya pasti FTS.
        var lewatIlike = await HitungSqlAsync(
            host,
            """
            SELECT count(*)::int AS "Value" FROM technology.technologies
            WHERE "Slug" = {0} AND ("Name" ILIKE {1} OR "Summary" ILIKE {1})
            """,
            slug,
            "%pelantur zarqun%");

        Assert.Equal(0, lewatIlike);
    }

    [Fact]
    public async Task Mengetik_sebagian_kata_tetap_ketemu()
    {
        using var host = Host();
        using var client = host.CreateClient();

        var slug = await BuatTopikAsync(host, "Zarqunimetri", "Catatan uji untuk pencarian.");

        // Orang mengetik sambil berpikir; "zarqunim" adalah keadaan normal
        // sebuah kotak pencarian, bukan kasus tepi.
        var hasil = await CariAsync(client, "zarqunim");

        Assert.Contains(hasil.Technologies.Items, t => t.Slug == slug);

        // 🔑 KENDALI ARAH SEBALIKNYA. FTS mencocokkan KATA UTUH, jadi di sini ia
        // memang tidak bisa — dan itulah alasan ILIKE tidak dibuang. Kalau suatu
        // saat baris ini jadi 1 (misalnya karena pencarian awalan dipasang),
        // ujinya merah dan keputusan ADR-022 harus ditinjau ulang, bukan
        // diam-diam usang.
        var lewatFts = await HitungSqlAsync(
            host,
            """
            SELECT count(*)::int AS "Value" FROM technology.technologies
            WHERE "Slug" = {0} AND search_vector @@ websearch_to_tsquery('english', {1})
            """,
            slug,
            "zarqunim");

        Assert.Equal(0, lewatFts);
    }

    [Theory]
    [InlineData("zarqun &")]
    [InlineData("& | !")]
    [InlineData("(")]
    [InlineData("'")]
    public async Task Masukan_yang_bukan_kueri_membalas_200_bukan_500(string q)
    {
        using var host = Host();
        using var client = host.CreateClient();

        var response = await client.GetAsync(
            new Uri($"/api/v1/search?q={Uri.EscapeDataString(q)}", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    /// Parameter kueri yang tidak bisa diikat membalas 400 yang menyebut
    /// parameternya — di kedua lingkungan (issue #61).
    /// </summary>
    /// <remarks>
    /// 🔴 Ini permukaan BACA, yang TAYANG di produksi (ADR-020 hanya menutup yang
    /// menulis). Terukur 2026-09-17 sebelum perbaikan: <c>?pageSize=abc</c> membalas
    /// 500 di <c>Development</c>, dan 400 tanpa satu kata pun tentang sebabnya di
    /// <c>Production</c> — dua jalan ASP.NET yang berbeda untuk permintaan yang sama.
    /// Sakelar tulis sengaja tidak dinyalakan: bentuk produksi.
    /// </remarks>
    [Theory]
    [InlineData("Development", "/api/v1/technologies?pageSize=abc", "pageSize")]
    [InlineData("Production", "/api/v1/technologies?pageSize=abc", "pageSize")]
    [InlineData("Development", "/api/v1/search?q=zarqun&page=abc", "page")]
    [InlineData("Production", "/api/v1/search?q=zarqun&page=abc", "page")]
    public async Task Parameter_kueri_yang_tidak_bisa_diikat_membalas_400_yang_menyebut_parameternya(
        string lingkungan,
        string alamat,
        string parameter)
    {
        using var host = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment(lingkungan);
            builder.UseSetting("ConnectionStrings:Postgres", Postgres);
            builder.UseSetting("ConnectionStrings:Redis", string.Empty);
        });
        using var client = host.CreateClient();

        var response = await client.GetAsync(new Uri(alamat, UriKind.Relative));
        var teks = await response.Content.ReadAsStringAsync();

        Assert.True(response.StatusCode == HttpStatusCode.BadRequest, $"{lingkungan} {alamat} -> {(int)response.StatusCode} {teks}");
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        using var dokumen = JsonDocument.Parse(teks);
        var detail = dokumen.RootElement.TryGetProperty("detail", out var d) ? d.GetString() : null;
        Assert.True(
            detail is not null
            && detail.Contains(parameter, StringComparison.Ordinal)
            && detail.Contains("abc", StringComparison.Ordinal),
            $"detail harus menyebut parameter '{parameter}' dan nilainya. Badan: {teks}");
    }

    [Fact]
    public async Task Kendali_to_tsquery_MEMANG_meledak_pada_masukan_yang_sama()
    {
        // 🔑 Ini yang membuat uji di atasnya berarti. Tanpa kendali ini, "200"
        // cuma berarti "hari ini tidak meledak" — tidak ada yang membuktikan
        // bahwa pilihan websearch_to_tsquery-lah yang mencegahnya.
        //
        // to_tsquery adalah fungsi yang dipakai hampir semua contoh FTS di
        // internet. Di kotak pencarian publik ia berarti HTTP 500 yang dipicu
        // satu karakter yang diketik pengunjung.
        using var host = Host();

        await Assert.ThrowsAnyAsync<DbException>(() => HitungSqlAsync(
            host,
            """SELECT count(*)::int AS "Value" FROM technology.technologies WHERE search_vector @@ to_tsquery('english', {0})""",
            "zarqun &"));
    }

    [Fact]
    public async Task Kecocokan_di_nama_menang_atas_kecocokan_di_ringkasan()
    {
        using var host = Host();
        using var client = host.CreateClient();

        // Nama yang mengandung kata kuncinya sengaja diberi huruf awal PALING
        // BELAKANG dalam abjad. Kalau peringkatnya tidak bekerja, pemutus seri
        // (nama menaik) akan menaruh "Aaa…" di atas — jadi uji ini tidak bisa
        // hijau karena kebetulan urutannya.
        var diRingkasan = await BuatTopikAsync(host, "Aaa Uji Peringkat", "Membahas pelanturium sampai tuntas.");
        var diNama = await BuatTopikAsync(host, "Zzz Pelanturium", "Catatan uji untuk pencarian.");

        var hasil = await CariAsync(client, "pelanturium");

        var urutan = hasil.Technologies.Items.Select(t => t.Slug).ToList();
        var posisiNama = urutan.IndexOf(diNama);
        var posisiRingkasan = urutan.IndexOf(diRingkasan);

        // Yang diperiksa URUTAN RELATIF keduanya, bukan panjang daftarnya.
        // Menuntut "tepat dua hasil" akan menjadikan uji ini bergantung pada
        // basis data yang bersih - dan di mesin pengembang basis datanya sama
        // dengan yang dipakai run.ps1 web.
        Assert.True(posisiNama >= 0, "topik dengan kata kunci di NAMA tidak ketemu sama sekali");
        Assert.True(posisiRingkasan >= 0, "topik dengan kata kunci di RINGKASAN tidak ketemu sama sekali");
        Assert.True(
            posisiNama < posisiRingkasan,
            $"kecocokan di nama seharusnya di atas kecocokan di ringkasan; urutannya: {string.Join(", ", urutan)}");
    }

    [Fact]
    public async Task Bidang_ikut_menjaring_topik_di_bawahnya()
    {
        using var host = Host();
        using var client = host.CreateClient();

        // Nama dan ringkasannya sama sekali tidak menyebut "quantum" — satu-satunya
        // hubungannya dengan kata itu adalah BIDANG tempat ia duduk.
        var slug = await BuatTopikAsync(
            host,
            "Zarqun Pelantur Dua",
            "Catatan uji untuk pencarian.",
            fieldSlug: "quantum-computing");

        var hasil = await CariAsync(client, "quantum");

        // Bidangnya sendiri ikut terjawab: ia punya halaman, jadi ia hasil yang sah.
        Assert.Contains(hasil.Fields, f => f.Slug == "quantum-computing");

        // Dan topik di bawahnya ikut terjaring, walau teksnya sendiri tidak
        // menyebut kata itu. Ini yang membuat pencarian berguna selama isi situs
        // masih tipis: taksonomi ADR-010 adalah cara utama isinya ditemukan.
        Assert.Contains(hasil.Technologies.Items, t => t.Slug == slug);
    }

    [Fact]
    public async Task Bentuk_jamak_Inggris_ikut_ketemu()
    {
        using var host = Host();
        using var client = host.CreateClient();

        var slug = await BuatTopikAsync(host, "Zarqun Agents", "Catatan uji untuk pencarian.");

        // Inilah yang dibeli kamus 'english' dan tidak diberi 'simple':
        // agents → agent. Nama bidang di ADR-010 hampir seluruhnya Inggris.
        //
        // Kata kuncinya sengaja DIBALIK juga ("agent zarqun"), sebab
        // "zarqun agent" kebetulan substring dari "Zarqun Agents" — ILIKE akan
        // menemukannya, dan ujinya jadi hijau tanpa stemming ikut bekerja.
        var hasil = await CariAsync(client, "agent zarqun");

        Assert.Contains(hasil.Technologies.Items, t => t.Slug == slug);

        // 🔑 KENDALI: jalur lama memang tidak sanggup.
        var lewatIlike = await HitungSqlAsync(
            host,
            """
            SELECT count(*)::int AS "Value" FROM technology.technologies
            WHERE "Slug" = {0} AND ("Name" ILIKE {1} OR "Summary" ILIKE {1})
            """,
            slug,
            "%agent zarqun%");

        Assert.Equal(0, lewatIlike);
    }

    [Fact]
    public async Task Mencari_iot_TIDAK_memulangkan_Biotechnology()
    {
        using var host = Host();
        using var client = host.CreateClient();

        // 🔴 Uji ini lahir dari MENJALANKAN, bukan dari membaca kode. Cadangan
        // pencocokan sebagian mula-mula ILIKE '%iot%', dan itu memulangkan
        // bidang Biotechnology — b-IOT-echnology. "iot" bukan kata kunci aneh:
        // ia nama salah satu dari empat belas bidang ADR-010.
        //
        // Keduanya bidang bawaan migrasi, jadi uji ini tidak membuat apa pun dan
        // berlaku sama di CI yang basis datanya lahir kosong.
        var hasil = await CariAsync(client, "iot");

        Assert.Contains(hasil.Fields, f => f.Slug == "iot");
        Assert.DoesNotContain(hasil.Fields, f => f.Slug == "biotechnology");

        // KENDALI: "edge-ai" HARUS tetap ada. Ringkasannya menyebut IoT sebagai
        // kata utuh ("BUKAN anak IoT"), jadi kalau ia ikut hilang, yang terjadi
        // bukan "noise-nya hilang" melainkan pencocokannya mati sama sekali.
        Assert.Contains(hasil.Fields, f => f.Slug == "edge-ai");
    }

    [Fact]
    public async Task Kata_kunci_kosong_mengembalikan_dua_daftar_kosong()
    {
        using var host = Host();
        using var client = host.CreateClient();

        var hasil = await CariAsync(client, "   ");

        // Bukan seluruh isi situs. Halaman pencarian yang belum ditanyai apa-apa
        // adalah halaman yang belum punya jawaban.
        Assert.Empty(hasil.Fields);
        Assert.Empty(hasil.Technologies.Items);
        Assert.True(hasil.IsEmpty);
        Assert.Equal(string.Empty, hasil.Query);
    }

    [Fact]
    public async Task Amplop_kosong_memakai_pageSize_yang_SAMA_dengan_amplop_berisi()
    {
        using var host = Host();
        using var client = host.CreateClient();

        // Satu parameter, satu jawaban. Tanpa penjaga ini, cabang "tidak ada kata
        // kunci" gampang merakit amplopnya sendiri dari angka MENTAH di URL —
        // dan klien yang membaca pageSize akan melihat dua nilai berbeda untuk
        // permintaan yang sama-sama ?pageSize=99999.
        var kosong = await CariAsync(client, "   ");
        var berisi = await CariAsync(client, "quantum");

        var kosongBesar = await client.GetFromJsonAsync<SearchResponse>(
            new Uri("/api/v1/search?q=%20&pageSize=99999", UriKind.Relative));
        var berisiBesar = await client.GetFromJsonAsync<SearchResponse>(
            new Uri("/api/v1/search?q=quantum&pageSize=99999", UriKind.Relative));

        Assert.Equal(berisi.Technologies.PageSize, kosong.Technologies.PageSize);
        Assert.Equal(berisiBesar!.Technologies.PageSize, kosongBesar!.Technologies.PageSize);

        // Dan angka itu memang dibatasi, bukan diteruskan apa adanya.
        Assert.Equal(SearchTechnologyQuery.MaxPageSize, berisiBesar.Technologies.PageSize);
    }

    [Fact]
    public async Task Vektor_dihitung_PostgreSQL_sendiri_bukan_oleh_kode()
    {
        using var host = Host();
        using var client = host.CreateClient();

        var slug = await BuatTopikAsync(host, "Zarqun Lama", "Catatan uji untuk pencarian.");

        // Nama diganti lewat SQL MENTAH — menembus agregat, menembus EF, menembus
        // seluruh kode C#. Kalau vektornya diisi kode aplikasi, tidak ada satu pun
        // yang memperbaruinya di sini dan pencarian di bawah akan nol.
        using (var scope = host.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TechnologyDbContext>();
            var baris = await db.Database.ExecuteSqlRawAsync(
                """UPDATE technology.technologies SET "Name" = {1} WHERE "Slug" = {0}""",
                slug,
                "Zarqun Pelanturi");

            Assert.Equal(1, baris);
        }

        var hasil = await CariAsync(client, "pelanturi");

        Assert.Contains(hasil.Technologies.Items, t => t.Slug == slug);

        // Dan nama lamanya berhenti cocok — vektornya benar-benar dihitung ulang,
        // bukan sekadar ditambahi.
        var lama = await CariAsync(client, "zarqun lama");
        Assert.DoesNotContain(lama.Technologies.Items, t => t.Slug == slug);
    }
}
