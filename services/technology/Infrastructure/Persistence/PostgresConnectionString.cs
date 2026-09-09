namespace TechVerseX.TechnologyService.Infrastructure.Persistence;

/// <summary>
/// Menerima string koneksi Postgres dalam <b>dua bentuk</b> dan mengembalikan
/// bentuk yang dimengerti Npgsql.
/// </summary>
/// <remarks>
/// 🔑 <b>Ini kejutan penyebaran yang ditemukan sebelum menyebarkan.</b>
/// <c>RENCANA-V1.md</c> menyebut "migrasi di lingkungan asing" sebagai salah satu
/// kejutan yang menunggu; yang ini tetangganya.
/// <para>
/// Hampir setiap platform terkelola — Render, Railway, Heroku, Neon, Supabase —
/// menyerahkan kredensial basis data sebagai <b>URI</b>
/// (<c>postgresql://user:sandi@host:5432/nama</c>), sedangkan Npgsql menuntut
/// bentuk kunci-nilai (<c>Host=...;Port=...;Username=...</c>) dan
/// <b>MELEMPAR</b> untuk URI. Tanpa penerjemah ini, menyalin nilai yang diberikan
/// platform apa adanya akan gagal saat start — bukan saat konfigurasi.
/// </para>
/// <para>
/// Karena itu ia juga <b>menjaga janji "citra tidak mengunci platform"</b>
/// (ADR-017): bentuk URI adalah bentuk yang paling banyak dipakai platform, dan
/// mendukungnya berarti pindah platform tidak menuntut perubahan kode.
/// </para>
/// </remarks>
public static class PostgresConnectionString
{
    /// <summary>
    /// Mengembalikan string kunci-nilai untuk Npgsql. Masukan yang sudah
    /// berbentuk kunci-nilai dikembalikan apa adanya.
    /// </summary>
    public static string Normalize(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var trimmed = value.Trim();

        // Bukan URI? Berarti sudah bentuk kunci-nilai. Jangan disentuh - menebak
        // lebih jauh di sini hanya akan merusak string yang sudah benar.
        if (!trimmed.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
            && !trimmed.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        {
            return trimmed;
        }

        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
        {
            throw new ArgumentException(
                "ConnectionStrings:Postgres terlihat seperti URI tapi tidak bisa diurai.", nameof(value));
        }

        var userInfo = uri.UserInfo.Split(':', 2);
        var username = Uri.UnescapeDataString(userInfo[0]);
        var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;

        // Port -1 berarti URI-nya tidak menyebut port; 5432 default Postgres.
        var port = uri.IsDefaultPort || uri.Port <= 0 ? 5432 : uri.Port;
        var database = uri.AbsolutePath.TrimStart('/');

        var builder = new Npgsql.NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = port,
            Database = database,
            Username = username,
            Password = password,
        };

        // sslmode ikut kalau URI-nya menyebutkannya. Platform terkelola sering
        // menuntut TLS, dan membuang parameter ini diam-diam akan menghasilkan
        // kegagalan koneksi yang sebabnya tidak kelihatan.
        foreach (var pasangan in ParseQuery(uri.Query))
        {
            var kunci = KeywordNpgsql(pasangan.Key);

            try
            {
                builder[kunci] = pasangan.Value;
            }
            catch (ArgumentException ex)
            {
                // Pesan asli Npgsql hanya berbunyi "Couldn't set <nama>", dan EF
                // membungkusnya lagi di balik "Continuing without the application
                // service provider" - dua lapis yang membuat sebabnya nyaris tak
                // terbaca. Sebutkan parameternya, dan sebutkan bahwa yang salah
                // BUKAN string dari platformnya.
                throw new ArgumentException(
                    $"Parameter '{pasangan.Key}' pada URI ConnectionStrings:Postgres tidak dikenali Npgsql. "
                    + "Jangan menyunting string yang diberikan platform; laporkan parameter ini supaya "
                    + "penerjemahnya yang diperbaiki.",
                    nameof(value),
                    ex);
            }
        }

        return builder.ConnectionString;
    }

    /// <summary>
    /// Menerjemahkan nama parameter gaya libpq ke nama yang dikenali Npgsql.
    /// </summary>
    /// <remarks>
    /// 🔴 <b>Ini yang menghentikan penyebaran pertama ke Neon, dan sebabnya satu
    /// karakter.</b> libpq menulis parameternya dengan <b>garis bawah</b>
    /// (<c>channel_binding</c>, <c>application_name</c>), sedangkan Npgsql
    /// mencocokkan kata kunci dengan mengabaikan huruf besar-kecil <b>dan spasi
    /// — tapi TIDAK garis bawah</b>. Karena itu <c>sslmode</c> lolos (tak punya
    /// pemisah sama sekali) sementara <c>channel_binding</c> ditolak, walau
    /// Npgsql punya properti <c>Channel Binding</c> untuk persis parameter itu.
    /// <para>
    /// Neon menyerahkan <c>?sslmode=require&amp;channel_binding=require</c> apa
    /// adanya di dasbornya, dan <c>PENYEBARAN.md</c> melarang merakit ulang
    /// string koneksi dengan tangan. Tanpa terjemahan ini, kedua larangan itu
    /// bertabrakan dan yang kalah adalah penyebarannya.
    /// </para>
    /// </remarks>
    private static string KeywordNpgsql(string keyword) => keyword.Replace('_', ' ');

    private static IEnumerable<KeyValuePair<string, string>> ParseQuery(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            yield break;
        }

        foreach (var bagian in query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var kv = bagian.Split('=', 2);
            if (kv.Length == 2)
            {
                yield return new KeyValuePair<string, string>(
                    Uri.UnescapeDataString(kv[0]).ToLowerInvariant(),
                    Uri.UnescapeDataString(kv[1]));
            }
        }
    }
}
