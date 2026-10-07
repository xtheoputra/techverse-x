namespace TechVerseX.TechnologyService.Infrastructure.Persistence;

/// <summary>
/// Satu tempat untuk seluruh angka dan nama yang dipakai pencarian teks penuh.
/// </summary>
/// <remarks>
/// Nilai-nilai di sini muncul di TIGA tempat yang tidak saling melihat: ekspresi
/// kolom terhitung di konfigurasi EF, SQL di berkas migrasi, dan pemanggilan
/// <c>websearch_to_tsquery</c> di handler. Kalau ketiganya tidak memakai
/// konfigurasi teks yang sama persis, gejalanya <b>bukan galat</b> melainkan
/// pencarian yang diam-diam meleset: dokumen disimpan sebagai <c>'comput'</c>
/// sementara kueri mencari <c>'computing'</c>.
/// <para>
/// Lihat <see href="../../../../docs/adr/ADR-022-pencarian-teks-penuh.md">ADR-022</see>
/// untuk alasan tiap pilihan, berikut pengukuran yang mendasarinya.
/// </para>
/// </remarks>
internal static class PencarianTeks
{
    /// <summary>
    /// Konfigurasi kamus PostgreSQL yang dipakai, di index maupun di kueri.
    /// </summary>
    /// <remarks>
    /// <b><c>english</c>, dan itu DIUKUR di korpus sungguhan — bukan dipilih
    /// karena isinya berbahasa Inggris.</b> PostgreSQL tidak membawa kamus bahasa
    /// Indonesia sama sekali, jadi pilihannya cuma <c>simple</c> (tanpa stemming,
    /// tanpa stopword) atau kamus bahasa lain. Yang diukur pada 2026-09-11 atas
    /// isi <c>FieldCatalog</c> + contoh <c>run.ps1 seed</c>:
    /// <list type="bullet">
    /// <item><description>
    /// Kata Indonesia yang HILANG karena dianggap stopword Inggris: <b>nol</b>.
    /// Tiga kata yang hilang — <c>in</c>, <c>on</c>, <c>the</c> — memang kata
    /// Inggris, datang dari "on-device" dan "tinyML Foundation".
    /// </description></item>
    /// <item><description>
    /// Kata korpus yang BERBEDA lexeme-nya antara <c>english</c> dan
    /// <c>simple</c>: 44 dari 206. Yang Indonesia dipotong secara simetris
    /// (<c>akses</c> → <c>aks</c>), dan simetris berarti tidak merusak: potongan
    /// yang sama berlaku di kueri.
    /// </description></item>
    /// <item><description>
    /// Tabrakan stemmer di korpus: <b>satu</b>, yaitu
    /// <c>computer</c> + <c>computing</c> → <c>comput</c>. Itu tabrakan yang
    /// memang diinginkan.
    /// </description></item>
    /// <item><description>
    /// Yang DIBELI: <c>agent</c> menemukan "AI Agents", <c>container</c>
    /// menemukan "containers". Dengan <c>simple</c> keduanya nol — dan nama
    /// bidang di ADR-010 hampir seluruhnya bahasa Inggris.
    /// </description></item>
    /// </list>
    /// ⚠️ Mengubah nilai ini <b>wajib</b> disertai migrasi: kolom terhitungnya
    /// menyimpan hasil stemming, jadi isi lama tidak ikut berubah sendiri.
    /// </remarks>
    public const string Konfigurasi = "english";

    /// <summary>
    /// Nama properti bayangan yang menyimpan <c>tsvector</c>-nya.
    /// </summary>
    /// <remarks>
    /// <b>Properti bayangan, bukan properti di agregat.</b> <c>NpgsqlTsVector</c>
    /// tipe milik driver; menaruhnya di <see cref="Domain.Technology"/> berarti
    /// akar agregat mengetahui basis datanya. Ia juga tidak punya arti domain —
    /// tidak ada aturan bisnis yang membacanya — jadi tempatnya memang di lapisan
    /// persistensi.
    /// </remarks>
    public const string KolomVektor = "SearchVector";

    /// <summary>Nama kolomnya di basis data.</summary>
    public const string NamaKolom = "search_vector";

    /// <summary>
    /// Ekspresi kolom terhitung, apa adanya seperti yang dikirim ke PostgreSQL.
    /// </summary>
    /// <remarks>
    /// 🔑 <b>Kolom <c>GENERATED ALWAYS ... STORED</c>, bukan trigger dan bukan
    /// kolom biasa yang diisi kode.</b> Kolom biasa bisa hanyut dari teks
    /// sumbernya — setiap jalan tulis baru wajib ingat memperbaruinya, dan yang
    /// lupa tidak menghasilkan galat, cuma baris yang tidak pernah ketemu.
    /// PostgreSQL yang menghitungnya berarti tidak ada jalan tulis yang bisa
    /// lupa.
    /// <para>
    /// ⚠️ <b>Ekspresinya wajib IMMUTABLE, dan itu yang memaksa bentuk dua
    /// argumen.</b> <c>to_tsvector(x)</c> memakai <c>default_text_search_config</c>
    /// yang bisa berubah per sesi, jadi PostgreSQL menolaknya di kolom terhitung.
    /// <c>to_tsvector('english', x)</c> immutable dan diterima. <c>coalesce</c>
    /// dipakai supaya kolom kosong tidak membuat seluruh vektor NULL.
    /// </para>
    /// <para>
    /// Bobot <c>A</c> untuk nama dan <c>B</c> untuk ringkasan: kecocokan di nama
    /// halaman hampir selalu lebih berarti daripada kecocokan di paragraf
    /// ringkasannya. Terukur — <c>ts_rank</c> memberi 0,61 untuk yang pertama dan
    /// 0,24 untuk yang kedua.
    /// </para>
    /// </remarks>
    public const string Ekspresi = $"""
        setweight(to_tsvector('{Konfigurasi}', coalesce("Name", '')), 'A') || setweight(to_tsvector('{Konfigurasi}', coalesce("Summary", '')), 'B')
        """;

    /// <summary>Nama indeks GIN di tabel <c>technologies</c>.</summary>
    public const string IndeksTechnologies = "ix_technologies_search_vector";

    /// <summary>Nama indeks GIN di tabel <c>fields</c>.</summary>
    public const string IndeksFields = "ix_fields_search_vector";

    /// <summary>
    /// Membersihkan kata kunci yang diketik orang, dan menjawab apakah masih ada
    /// yang tersisa untuk dicari.
    /// </summary>
    /// <remarks>
    /// Batas panjangnya bukan hiasan: <c>websearch_to_tsquery</c> memang tidak
    /// pernah melempar, tapi kalimat sepanjang satu paragraf tetap menghasilkan
    /// tsquery raksasa yang dipindai GIN untuk hasil yang tidak berguna.
    /// </remarks>
    public static bool Bersihkan(string? mentah, out string bersih)
    {
        bersih = mentah?.Trim() ?? string.Empty;

        if (bersih.Length > MaksPanjangKataKunci)
        {
            bersih = bersih[..MaksPanjangKataKunci];
        }

        return bersih.Length > 0;
    }

    /// <summary>Panjang maksimum kata kunci yang diterima, dalam karakter.</summary>
    public const int MaksPanjangKataKunci = 200;

    /// <summary>
    /// Pola POSIX untuk cadangan "orang baru mengetik separuh kata": cocok kalau
    /// kata kuncinya berada di <b>AWAL sebuah kata</b>.
    /// </summary>
    /// <remarks>
    /// 🔴 <b>Ini menggantikan <c>ILIKE '%kata%'</c>, dan penggantinya lahir dari
    /// satu hasil yang dijalankan, bukan dari selera:</b> mencari <c>iot</c>
    /// memulangkan bidang <b>Biotechnology</b> — sebab <c>b-iot-echnology</c>
    /// memang memuat potongan huruf itu. <c>iot</c> bukan kata kunci aneh; ia
    /// nama salah satu bidang.
    /// <para>
    /// Cadangan ini ada untuk <b>pengetikan sebagian</b>, dan orang mengetik
    /// <em>awal</em> kata, bukan tengahnya. <c>\m</c> adalah jangkar "awal kata"
    /// POSIX, dan ia jauh lebih tepat daripada mencocokkan spasi sendiri: ia ikut
    /// mengenali batas kata di <c>/</c> dan <c>-</c>, jadi <c>thread</c> tetap
    /// menemukan <c>Matter/Thread</c>.
    /// </para>
    /// <para>
    /// ⚠️ Sama seperti <c>ILIKE</c> yang digantikannya, pencocokan ini
    /// <b>tidak memakai indeks</b> — pemindaian berurutan. Alasannya tidak
    /// berubah: pada ukuran V1 (14 bidang, 82 halaman di seluruh taksonomi) itu
    /// tidak terukur. Lihat ADR-022 poin 4.
    /// </para>
    /// <para>
    /// 🔑 <c>Regex.Escape</c> bukan kemewahan. Tanpa itu, pengunjung yang
    /// mengetik <c>(</c> mengirim pola yang tidak sah ke PostgreSQL — persis
    /// bentuk kegagalan yang dihindari dengan memilih <c>websearch_to_tsquery</c>
    /// di sisi FTS-nya.
    /// </para>
    /// </remarks>
    public static string PolaAwalKata(string bersih)
        => @"\m" + System.Text.RegularExpressions.Regex.Escape(bersih);
}
