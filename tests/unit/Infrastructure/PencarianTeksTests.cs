using TechVerseX.TechnologyService.Infrastructure.Persistence;

namespace TechVerseX.TechnologyService.Tests.Infrastructure;

/// <summary>
/// Pembersih kata kunci pencarian.
/// </summary>
/// <remarks>
/// 🔑 <b>Yang dijaga di sini bukan "spasi terpangkas", melainkan bahwa jawaban
/// <c>false</c> benar-benar berarti "tidak ada yang dicari".</b> Seluruh jalur
/// pencarian bercabang di nilai itu: <c>false</c> membuat
/// <c>SearchTechnologyHandler</c> mengembalikan DAFTAR (tanpa peringkat) dan
/// membuat <c>SearchHandler</c> mengembalikan dua daftar KOSONG. Kalau spasi
/// kosong lolos sebagai kata kunci, halaman pencarian yang belum ditanyai apa-apa
/// akan menampilkan seluruh isi situs seolah itu hasil pencarian.
/// </remarks>
public sealed class PencarianTeksTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\n")]
    public void Kata_kunci_kosong_menjawab_false(string? mentah)
    {
        Assert.False(PencarianTeks.Bersihkan(mentah, out var bersih));
        Assert.Equal(string.Empty, bersih);
    }

    [Fact]
    public void Spasi_di_pinggir_dibuang()
    {
        Assert.True(PencarianTeks.Bersihkan("  quantum computing \n", out var bersih));
        Assert.Equal("quantum computing", bersih);
    }

    [Fact]
    public void Kata_kunci_kepanjangan_dipotong_di_batasnya()
    {
        var mentah = new string('a', PencarianTeks.MaksPanjangKataKunci + 50);

        Assert.True(PencarianTeks.Bersihkan(mentah, out var bersih));
        Assert.Equal(PencarianTeks.MaksPanjangKataKunci, bersih.Length);
    }

    [Fact]
    public void Kata_kunci_tepat_sepanjang_batas_TIDAK_dipotong()
    {
        // Kendali untuk uji di atasnya. Tanpa baris ini, pemotongan yang keliru
        // satu karakter — `< ` yang seharusnya `<=` — tetap hijau di kedua sisi.
        var mentah = new string('a', PencarianTeks.MaksPanjangKataKunci);

        Assert.True(PencarianTeks.Bersihkan(mentah, out var bersih));
        Assert.Equal(mentah, bersih);
    }

    [Fact]
    public void Konfigurasi_teks_tertulis_SATU_kali()
    {
        // Ekspresi kolom terhitung disusun dari konstanta yang sama dengan yang
        // dipakai websearch_to_tsquery di handler. Kalau keduanya pernah
        // berpisah, gejalanya bukan galat melainkan pencarian yang diam-diam
        // meleset: dokumen tersimpan sebagai 'comput', kueri mencari 'computing'.
        Assert.Contains($"to_tsvector('{PencarianTeks.Konfigurasi}'", PencarianTeks.Ekspresi, StringComparison.Ordinal);

        // Dua kolom sumber, dua bobot. Bobotnya yang membuat kecocokan di nama
        // menang atas kecocokan di ringkasan.
        Assert.Contains("'A'", PencarianTeks.Ekspresi, StringComparison.Ordinal);
        Assert.Contains("'B'", PencarianTeks.Ekspresi, StringComparison.Ordinal);
    }
}
