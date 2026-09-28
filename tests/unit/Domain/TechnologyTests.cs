using TechVerseX.TechnologyService.Domain;

namespace TechVerseX.TechnologyService.Tests.Domain;

public sealed class TechnologyTests
{
    private static readonly Guid AnyField = FieldCatalog.All[0].Id;

    [Fact]
    public void Create_MenurunkanSlugDariNamaSaatSlugTidakDiberikan()
    {
        var technology = Technology.Create("Edge AI", "AI yang berjalan di perangkat.", AnyField);

        Assert.Equal("edge-ai", technology.Slug);
        Assert.Equal(TechnologyStatus.Draft, technology.Status);
    }

    [Fact]
    public void Create_MenerbitkanEventTechnologyCreated()
    {
        var technology = Technology.Create("Digital Twin", "Kembaran digital objek nyata.", AnyField);

        var created = Assert.Single(technology.Events);
        Assert.Equal(nameof(TechnologyCreated), created.EventType);
        Assert.Equal(1, created.Version);
    }

    [Fact]
    public void Create_MenolakNamaKosong()
    {
        Assert.Throws<ArgumentException>(() => Technology.Create("   ", "ringkasan", AnyField));
    }

    [Fact]
    public void Create_MenolakTeknologiTanpaBidang()
    {
        // Dulu kolom ini teks bebas dan boleh berisi apa saja - termasuk
        // "Belum diputuskan", yang memang dipakai contoh seed lama. Setelah
        // taksonomi ditutup (ADR-010), tidak ada lagi topik tanpa rumah.
        Assert.Throws<ArgumentException>(() => Technology.Create("Yatim", "Tanpa bidang.", Guid.Empty));
    }

    [Fact]
    public void Create_LahirSebagaiKurasi()
    {
        // ADR-012: semua halaman lahir sebagai kurasi tautan, lalu naik.
        var technology = Technology.Create("Matter", "Standar rumah pintar.", AnyField);

        Assert.Equal(ContentMaturity.Curated, technology.Maturity);
        Assert.Null(technology.ReviewedAt);
        Assert.Null(technology.ReviewedBy);
    }

    [Fact]
    public void Publish_MenolakEntriYangBaruDitemukanAgenDanBelumDiverifikasi()
    {
        // Ini penjaga yang paling penting di kelas ini. Pipeline riset di
        // KERANGKA.md 4.6 menaruh Verification SEBELUM Publish justru supaya
        // temuan agen yang berhalusinasi tidak bisa langsung tayang.
        var technology = Technology.Create("Drone Swarm", "Ratusan drone sebagai satu kesatuan.", AnyField);
        SetStatus(technology, TechnologyStatus.Discovered);

        var error = Assert.Throws<InvalidOperationException>(technology.Publish);
        Assert.Contains("verifikasi", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Publish_MengizinkanEntriDraftDanMencatatEventnya()
    {
        var technology = Technology.Create("Cybersecurity Berbasis AI", "Pertahanan otomatis.", AnyField);
        technology.ClearEvents();

        technology.Publish();

        Assert.Equal(TechnologyStatus.Published, technology.Status);
        Assert.Equal(nameof(TechnologyPublished), Assert.Single(technology.Events).EventType);
    }

    [Fact]
    public void Publish_DipanggilDuaKaliTidakMenerbitkanEventKedua()
    {
        var technology = Technology.Create("Humanoid Robot", "Robot berbentuk manusia.", AnyField);
        technology.Publish();
        technology.ClearEvents();

        technology.Publish();

        Assert.Empty(technology.Events);
    }

    [Fact]
    public void Publish_TIDAKMemeriksaKematanganIsi()
    {
        // Penjaga atas ADR-015 bagian 1: Status dan Maturity dua sumbu berbeda.
        // Halaman kurasi yang terbit adalah bentuk akhir yang BENAR untuk bidang
        // prioritas 3 - jadi menambahkan larangan di Publish() berdasarkan
        // Maturity adalah regresi, dan uji ini yang akan memerah kalau ada yang
        // mencobanya.
        var technology = Technology.Create("Space Debris", "Sampah orbit.", AnyField);

        technology.Publish();

        Assert.Equal(TechnologyStatus.Published, technology.Status);
        Assert.Equal(ContentMaturity.Curated, technology.Maturity);
    }

    private static void SetStatus(Technology technology, TechnologyStatus status)
    {
        typeof(Technology)
            .GetProperty(nameof(Technology.Status))!
            .SetValue(technology, status);
    }
}
