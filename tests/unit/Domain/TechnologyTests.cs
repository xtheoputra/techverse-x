using TechVerseX.TechnologyService.Domain;

namespace TechVerseX.TechnologyService.Tests.Domain;

public sealed class TechnologyTests
{
    [Theory]
    [InlineData("AI Agents", "ai-agents")]
    [InlineData("  Quantum   Computing  ", "quantum-computing")]
    [InlineData("Brain-Computer Interface (BCI)", "brain-computer-interface-bci")]
    [InlineData("AR/VR", "ar-vr")]
    [InlineData(".NET 10", "net-10")]
    [InlineData("Bioteknologi + AI", "bioteknologi-ai")]
    public void Slugify_MengubahNamaBidangJadiSlugYangAman(string input, string expected)
    {
        Assert.Equal(expected, Technology.Slugify(input));
    }

    [Fact]
    public void Slugify_MenolakNamaYangTidakMenyisakanKarakterApaPun()
    {
        Assert.Throws<ArgumentException>(() => Technology.Slugify("!!! ???"));
    }

    [Fact]
    public void Create_MenurunkanSlugDariNamaSaatSlugTidakDiberikan()
    {
        var technology = Technology.Create("Edge AI", "AI yang berjalan di perangkat.", "AI & Machine Learning");

        Assert.Equal("edge-ai", technology.Slug);
        Assert.Equal(TechnologyStatus.Draft, technology.Status);
    }

    [Fact]
    public void Create_MenerbitkanEventTechnologyCreated()
    {
        var technology = Technology.Create("Digital Twin", "Kembaran digital objek nyata.", "IoT");

        var created = Assert.Single(technology.Events);
        Assert.Equal(nameof(TechnologyCreated), created.EventType);
        Assert.Equal(1, created.Version);
    }

    [Fact]
    public void Create_MenolakNamaKosong()
    {
        Assert.Throws<ArgumentException>(() => Technology.Create("   ", "ringkasan", "Cloud"));
    }

    [Fact]
    public void Publish_MenolakEntriYangBaruDitemukanAgenDanBelumDiverifikasi()
    {
        // Ini penjaga yang paling penting di kelas ini. Pipeline riset di
        // KERANGKA.md 4.6 menaruh Verification SEBELUM Publish justru supaya
        // temuan agen yang berhalusinasi tidak bisa langsung tayang.
        var technology = Technology.Create("Drone Swarm", "Ratusan drone sebagai satu kesatuan.", "Robotics");
        SetStatus(technology, TechnologyStatus.Discovered);

        var error = Assert.Throws<InvalidOperationException>(technology.Publish);
        Assert.Contains("verifikasi", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Publish_MengizinkanEntriDraftDanMencatatEventnya()
    {
        var technology = Technology.Create("Cybersecurity Berbasis AI", "Pertahanan otomatis.", "Cybersecurity");
        technology.ClearEvents();

        technology.Publish();

        Assert.Equal(TechnologyStatus.Published, technology.Status);
        Assert.Equal(nameof(TechnologyPublished), Assert.Single(technology.Events).EventType);
    }

    [Fact]
    public void Publish_DipanggilDuaKaliTidakMenerbitkanEventKedua()
    {
        var technology = Technology.Create("Humanoid Robot", "Robot berbentuk manusia.", "Robotics");
        technology.Publish();
        technology.ClearEvents();

        technology.Publish();

        Assert.Empty(technology.Events);
    }

    private static void SetStatus(Technology technology, TechnologyStatus status)
    {
        typeof(Technology)
            .GetProperty(nameof(Technology.Status))!
            .SetValue(technology, status);
    }
}
