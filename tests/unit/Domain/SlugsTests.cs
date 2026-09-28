using TechVerseX.TechnologyService.Domain;

namespace TechVerseX.TechnologyService.Tests.Domain;

/// <summary>
/// Aturan slug URL — satu tempat untuk <see cref="Technology"/>, <see cref="Field"/>,
/// dan <see cref="Tool"/> (ADR-009).
/// </summary>
/// <remarks>
/// Sampai 2026-09-28 kedua uji ini tinggal di <c>TechnologyTests</c> dan memanggil
/// <c>Technology.Slugify</c>, penerus satu baris ke <see cref="Slugs.From"/> yang
/// tidak punya satu pun pemanggil produksi (#68). Yang diuji sejak awal memang
/// aturan <see cref="Slugs"/>; sekarang ujinya memanggilnya langsung.
/// </remarks>
public sealed class SlugsTests
{
    [Theory]
    [InlineData("AI Agents", "ai-agents")]
    [InlineData("  Quantum   Computing  ", "quantum-computing")]
    [InlineData("Brain-Computer Interface (BCI)", "brain-computer-interface-bci")]
    [InlineData("AR/VR", "ar-vr")]
    [InlineData(".NET 10", "net-10")]
    [InlineData("Bioteknologi + AI", "bioteknologi-ai")]
    public void From_MengubahNamaBidangJadiSlugYangAman(string input, string expected)
    {
        Assert.Equal(expected, Slugs.From(input));
    }

    [Fact]
    public void From_MenolakNamaYangTidakMenyisakanKarakterApaPun()
    {
        Assert.Throws<ArgumentException>(() => Slugs.From("!!! ???"));
    }
}
