using TechVerseX.TechnologyService.Domain;
using TechVerseX.TechnologyService.Features.CreateTechnology;

namespace TechVerseX.TechnologyService.Tests.Features;

/// <summary>
/// Penjaga batas 400-vs-500 di <c>POST /api/v1/technologies</c>.
/// </summary>
/// <remarks>
/// 🔴 Aturannya satu kalimat: <b>apa pun yang diluluskan validator ini harus bisa
/// dikerjakan sampai selesai.</b> Kalau validator meluluskan muatan yang kemudian
/// membuat lapisan di bawahnya melempar, yang sampai ke pemanggil adalah 500 —
/// padahal yang salah muatannya, bukan servernya.
/// <para>
/// Validator adalah SATU-SATUNYA penjaga sebelum handler: tidak ada satu pun
/// <c>try/catch</c> antara <c>CreateTechnologyEndpoint</c> dan
/// <c>app.UseExceptionHandler()</c>.
/// </para>
/// </remarks>
public sealed class CreateTechnologyValidatorTests
{
    private const int BatasKolomSlug = 160;

    private static readonly string FieldSlugSah = FieldCatalog.All[0].Slug;
    private static readonly Guid FieldIdSah = FieldCatalog.All[0].Id;

    private static readonly CreateTechnologyValidator Validator = new();

    /// <summary>Nama yang tidak menyisakan satu karakter pun untuk slug.</summary>
    [Theory]
    [InlineData("!!!")]
    [InlineData("??? ???")]
    [InlineData("---")]
    [InlineData("日本語")]
    public void Menolak_nama_yang_tidak_menyisakan_karakter_slug(string name)
    {
        var command = new CreateTechnologyCommand(name, "ringkasan", FieldSlugSah, null);

        Assert.False(Validator.Validate(command).IsValid);
    }

    /// <summary>Slug eksplisit yang isinya sampah.</summary>
    [Theory]
    [InlineData("!!!")]
    [InlineData("   -   ")]
    public void Menolak_slug_eksplisit_yang_tidak_menyisakan_karakter(string slug)
    {
        var command = new CreateTechnologyCommand("Edge AI", "ringkasan", FieldSlugSah, slug);

        Assert.False(Validator.Validate(command).IsValid);
    }

    /// <summary>
    /// Nama sepanjang 200 (batas <c>MaximumLength</c>) menurunkan slug sepanjang 200 —
    /// sedangkan kolomnya <c>varchar(160)</c>. Ini instans KEDUA dari cacat yang sama:
    /// muatan lolos validator, lalu gagal di lapisan bawah.
    /// </summary>
    [Fact]
    public void Menolak_nama_yang_slug_turunannya_melebihi_batas_kolom()
    {
        var name = new string('a', 200);

        // Prasyarat uji ini: slug turunannya memang melebihi kolom.
        Assert.True(Slugs.From(name).Length > BatasKolomSlug);

        var command = new CreateTechnologyCommand(name, "ringkasan", FieldSlugSah, null);

        Assert.False(Validator.Validate(command).IsValid);
    }

    /// <summary>Arah berlawanan: yang sah TIDAK boleh ikut tertolak.</summary>
    [Theory]
    [InlineData("Edge AI", null)]
    [InlineData("Brain-Computer Interface (BCI)", null)]
    [InlineData(".NET 10", null)]
    [InlineData("AR/VR", "ar-vr")]
    [InlineData("Rust", "  Rust Lang  ")]
    public void Meluluskan_masukan_yang_sah(string name, string? slug)
    {
        var command = new CreateTechnologyCommand(name, "ringkasan", FieldSlugSah, slug);

        var hasil = Validator.Validate(command);

        Assert.True(hasil.IsValid, string.Join(" | ", hasil.Errors.Select(e => e.ErrorMessage)));
    }

    /// <summary>
    /// Invarian yang sesungguhnya dijaga, dinyatakan langsung: setiap muatan yang
    /// diluluskan validator harus bisa dibuat domainnya tanpa melempar, dan slugnya
    /// harus muat di kolom.
    /// </summary>
    [Theory]
    [InlineData("!!!", null)]
    [InlineData("??? ???", null)]
    [InlineData("日本語", null)]
    [InlineData("Edge AI", "!!!")]
    [InlineData("Edge AI", null)]
    [InlineData("Quantum Computing", "quantum-computing")]
    public void Yang_diluluskan_validator_selalu_bisa_dikerjakan_sampai_selesai(string name, string? slug)
    {
        var command = new CreateTechnologyCommand(name, "ringkasan", FieldSlugSah, slug);

        if (!Validator.Validate(command).IsValid)
        {
            return; // Ditolak lebih dulu = 400. Itu jawaban yang benar.
        }

        var technology = Technology.Create(command.Name, command.Summary, FieldIdSah, command.Slug);

        Assert.True(technology.Slug.Length <= BatasKolomSlug);
    }
}
