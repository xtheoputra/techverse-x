using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace TechVerseX.Api.Tests;

/// <summary>
/// Gerbang kesiapan produksi: apakah API bisa hidup TANPA Redis.
/// </summary>
/// <remarks>
/// <para>
/// <see href="../../docs/adr/ADR-016-pagu-biaya.md">ADR-016</see> memutuskan Redis
/// <b>tidak di-provision di produksi V1</b> — ia harus membuktikan dirinya dulu.
/// ADR itu juga mencatat sendiri bahwa konsekuensinya "perubahan kode yang belum
/// ditulis dan belum diuji". Berkas ini uji itu.
/// </para>
/// <para>
/// 🔴 Kenapa ini gerbang produksi, bukan uji kenyamanan: platform peti kemas
/// memakai <c>/health/ready</c> untuk memutuskan apakah boleh mengalirkan lalu
/// lintas. Selama kesiapan menuntut Redis yang tidak ada, penyebaran tidak
/// pernah selesai — dan gejalanya menyamar jadi "aplikasinya lambat menyala".
/// </para>
/// <para>
/// ⚠️ Uji ini menyentuh PostgreSQL sungguhan (<c>run.ps1 up</c> di lokal, service
/// container di CI). Itu disengaja: kesiapan yang diuji tanpa dependensi sungguhan
/// tidak mengukur apa pun.
/// </para>
/// </remarks>
public sealed class HealthEndpointTests
{
    private const string Postgres =
        "Host=localhost;Port=5432;Database=techversex;Username=techversex;Password=techversex_dev";

    /// <summary>Port yang sengaja tidak ada penunggunya — Redis "tidak di-provision".</summary>
    private const string RedisYangTidakAda = "localhost:6399";

    private static WebApplicationFactory<Program> Host(string? redis) =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:Postgres", Postgres);

            // String kosong = tidak dikonfigurasi. Begitulah bentuk produksi V1.
            builder.UseSetting("ConnectionStrings:Redis", redis ?? string.Empty);
        });

    [Fact]
    public async Task Ready_hijau_saat_Redis_tidak_dikonfigurasi()
    {
        using var host = Host(redis: null);
        using var client = host.CreateClient();

        var response = await client.GetAsync(new Uri("/health/ready", UriKind.Relative));
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Postgres tetap diperiksa; Redis tidak boleh ikut disebut sama sekali,
        // karena check yang tidak dipasang tidak boleh muncul sebagai "sehat".
        Assert.Contains("postgres", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("redis", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Ready_membalas_503_bukan_500_saat_Redis_dikonfigurasi_tapi_mati()
    {
        using var host = Host(RedisYangTidakAda);
        using var client = host.CreateClient();

        var response = await client.GetAsync(new Uri("/health/ready", UriKind.Relative));
        var body = await response.Content.ReadAsStringAsync();

        // 500 berarti health check-nya sendiri yang meledak - pemanggil tidak
        // pernah tahu dependensi mana yang jatuh. 503 adalah jawaban yang benar,
        // dan badannya harus menyebutkan namanya.
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Contains("redis", body, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(RedisYangTidakAda)]
    public async Task Live_tetap_hidup_apa_pun_keadaan_dependensinya(string? redis)
    {
        using var host = Host(redis);
        using var client = host.CreateClient();

        var response = await client.GetAsync(new Uri("/health/live", UriKind.Relative));

        // Liveness sengaja TIDAK menyentuh dependensi - kalau ia ikut jatuh,
        // Postgres yang sedang bermasalah memicu restart yang percuma.
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
