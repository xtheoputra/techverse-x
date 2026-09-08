using Npgsql;
using TechVerseX.TechnologyService.Infrastructure.Persistence;

namespace TechVerseX.TechnologyService.Tests.Infrastructure;

/// <summary>
/// Penjaga bentuk string koneksi Postgres — kejutan penyebaran yang ditemukan
/// sebelum menyebarkan.
/// </summary>
/// <remarks>
/// Uji pertama di berkas ini bukan menguji kode kita, melainkan <b>membuktikan
/// bahwa penerjemahnya memang dibutuhkan</b>. Tanpa uji itu, seluruh berkas ini
/// bisa saja menjaga masalah yang tidak pernah ada.
/// </remarks>
public sealed class PostgresConnectionStringTests
{
    private const string UriRender =
        "postgresql://techversex:rahasia@dpg-abc123-a.oregon-postgres.render.com:5432/techversex_db";

    [Fact]
    public void Npgsql_MEMANG_menolak_bentuk_URI()
    {
        // Inilah alasan Normalize() ada. Kalau suatu hari Npgsql mulai menerima
        // URI, uji ini yang pertama memerah - dan penerjemahnya boleh dibuang.
        Assert.ThrowsAny<ArgumentException>(() => new NpgsqlConnectionStringBuilder(UriRender));
    }

    [Fact]
    public void URI_diterjemahkan_jadi_bentuk_kunci_nilai()
    {
        var hasil = PostgresConnectionString.Normalize(UriRender);

        var builder = new NpgsqlConnectionStringBuilder(hasil);
        Assert.Equal("dpg-abc123-a.oregon-postgres.render.com", builder.Host);
        Assert.Equal(5432, builder.Port);
        Assert.Equal("techversex_db", builder.Database);
        Assert.Equal("techversex", builder.Username);
        Assert.Equal("rahasia", builder.Password);
    }

    [Fact]
    public void Bentuk_kunci_nilai_dikembalikan_apa_adanya()
    {
        // Menebak lebih jauh pada string yang sudah benar hanya merusaknya.
        const string aslinya = "Host=localhost;Port=5432;Database=techversex;Username=techversex;Password=techversex_dev";

        Assert.Equal(aslinya, PostgresConnectionString.Normalize(aslinya));
    }

    [Theory]
    [InlineData("postgres://u:p@h/db")]
    [InlineData("postgresql://u:p@h/db")]
    [InlineData("POSTGRES://u:p@h/db")]
    public void Kedua_skema_dikenali_tanpa_peduli_huruf_besar_kecil(string uri)
    {
        var builder = new NpgsqlConnectionStringBuilder(PostgresConnectionString.Normalize(uri));

        Assert.Equal("h", builder.Host);
        Assert.Equal("db", builder.Database);

        // URI tanpa port harus jatuh ke 5432, bukan ke 0 atau -1.
        Assert.Equal(5432, builder.Port);
    }

    [Fact]
    public void Sandi_ber_karakter_khusus_ikut_didekode()
    {
        // Sandi acak dari platform sering memuat karakter yang harus di-escape di
        // URI. Kalau tidak didekode, koneksinya ditolak dengan pesan "password
        // authentication failed" - sebab yang paling menyesatkan yang bisa ada.
        var hasil = PostgresConnectionString.Normalize("postgresql://u:p%40ss%3Aword@h:5432/db");

        Assert.Equal("p@ss:word", new NpgsqlConnectionStringBuilder(hasil).Password);
    }

    [Fact]
    public void Parameter_kueri_seperti_sslmode_tidak_dibuang()
    {
        // Platform terkelola sering menuntut TLS. Membuang sslmode diam-diam
        // menghasilkan kegagalan koneksi yang sebabnya tidak kelihatan.
        var hasil = PostgresConnectionString.Normalize($"{UriRender}?sslmode=require");

        Assert.Equal(SslMode.Require, new NpgsqlConnectionStringBuilder(hasil).SslMode);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Masukan_kosong_ditolak(string value)
    {
        Assert.Throws<ArgumentException>(() => PostgresConnectionString.Normalize(value));
    }
}
