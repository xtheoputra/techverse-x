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

    [Fact]
    public void Npgsql_MEMANG_menolak_parameter_bergaris_bawah()
    {
        // Pasangan dari uji pertama berkas ini: sebelum menjaga terjemahannya,
        // buktikan dulu terjemahan itu memang dibutuhkan.
        //
        // Npgsql PUNYA properti untuk parameter ini - namanya "Channel Binding".
        // Yang tidak ia lakukan adalah mengabaikan GARIS BAWAH saat mencocokkan
        // kata kunci; spasi dan huruf besar-kecil diabaikan, garis bawah tidak.
        // Kalau suatu hari Npgsql ikut mengabaikannya, uji ini yang pertama
        // memerah - dan KeywordNpgsql boleh dibuang.
        var builder = new NpgsqlConnectionStringBuilder();

        Assert.ThrowsAny<ArgumentException>(() => builder["channel_binding"] = "require");

        // Bentuk berspasi diterima. Inilah yang membuat terjemahannya cukup
        // menukar satu karakter, bukan memelihara tabel padanan.
        builder["channel binding"] = "require";
        Assert.Equal(ChannelBinding.Require, builder.ChannelBinding);
    }

    [Fact]
    public void String_koneksi_Neon_APA_ADANYA_diterima()
    {
        // 🔴 Bentuk PERSIS yang dipajang dasbor Neon, disalin dari dokumentasinya
        // (neon.com/docs/connect/connect-from-any-app). Ia membawa DUA parameter,
        // dan yang kedua bergaris bawah.
        //
        // Ini bukan uji hipotetis: PENYEBARAN.md dan issue #38 sama-sama melarang
        // merakit ulang string koneksi dengan tangan, jadi bentuk inilah yang
        // benar-benar akan masuk ke secret NEON_DATABASE_URL.
        const string neon =
            "postgresql://alex:AbC123dEf@ep-cool-darkness-a1b2c3d4-pooler.us-east-2.aws.neon.tech/dbname"
            + "?sslmode=require&channel_binding=require";

        var builder = new NpgsqlConnectionStringBuilder(PostgresConnectionString.Normalize(neon));

        Assert.Equal("ep-cool-darkness-a1b2c3d4-pooler.us-east-2.aws.neon.tech", builder.Host);
        Assert.Equal("dbname", builder.Database);
        Assert.Equal("alex", builder.Username);
        Assert.Equal("AbC123dEf", builder.Password);

        // URI Neon tidak menyebut port sama sekali.
        Assert.Equal(5432, builder.Port);

        // Keduanya soal keamanan transport. Membuang salah satunya diam-diam
        // jauh lebih buruk daripada gagal terang-terangan.
        Assert.Equal(SslMode.Require, builder.SslMode);
        Assert.Equal(ChannelBinding.Require, builder.ChannelBinding);
    }

    [Fact]
    public void Parameter_bergaris_bawah_lain_ikut_diterjemahkan()
    {
        // Terjemahannya berlaku umum, bukan tambalan khusus Neon.
        var hasil = PostgresConnectionString.Normalize($"{UriRender}?application_name=techversex-api");

        Assert.Equal("techversex-api", new NpgsqlConnectionStringBuilder(hasil).ApplicationName);
    }

    [Fact]
    public void Parameter_yang_benar_benar_asing_ditolak_sambil_menyebut_namanya()
    {
        // Yang tidak dikenali TETAP ditolak - membuangnya diam-diam persis
        // kesalahan yang dihindari sepanjang berkas ini. Yang diperbaiki cuma
        // keterbacaannya: pesan Npgsql sendiri terkubur di balik dua lapis
        // pembungkus EF.
        var galat = Assert.Throws<ArgumentException>(
            () => PostgresConnectionString.Normalize($"{UriRender}?parameter_karangan=1"));

        Assert.Contains("parameter_karangan", galat.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Masukan_kosong_ditolak(string value)
    {
        Assert.Throws<ArgumentException>(() => PostgresConnectionString.Normalize(value));
    }
}
