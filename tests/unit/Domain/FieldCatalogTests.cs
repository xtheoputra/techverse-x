using TechVerseX.TechnologyService.Domain;

namespace TechVerseX.TechnologyService.Tests.Domain;

/// <summary>
/// Penjaga supaya daftar bidang di kode tidak hanyut dari ADR-010.
/// </summary>
/// <remarks>
/// Angka di dokumen dan angka di kode punya kebiasaan buruk: keduanya berubah
/// sendiri-sendiri. Uji ini mengikat keduanya, jadi menambah atau membuang satu
/// bidang tanpa membuka ADR-010 akan memerah di sini.
/// </remarks>
public sealed class FieldCatalogTests
{
    [Fact]
    public void Katalog_BerisiEmpatBelasBidang()
    {
        Assert.Equal(FieldCatalog.ExpectedCount, FieldCatalog.All.Count);
        Assert.Equal(14, FieldCatalog.All.Count);
    }

    [Fact]
    public void Katalog_PunyaEnamBidangPrioritasSatu()
    {
        // Enam bidang inilah yang 42 topiknya dijanjikan ditulis manusia
        // (docs/RENCANA-V1.md). Menaikkan bidang ke Core berarti menaikkan janji.
        var core = FieldCatalog.All.Where(f => f.Priority == FieldPriority.Core).ToList();

        Assert.Equal(6, core.Count);
        Assert.Contains(core, f => f.Slug == "ai-agents");
        Assert.Contains(core, f => f.Slug == "data-engineering");
        Assert.Contains(core, f => f.Slug == "iot");
    }

    [Fact]
    public void Katalog_PunyaDuaBidangYangSengajaTidakDiinvestasikan()
    {
        var peripheral = FieldCatalog.All.Where(f => f.Priority == FieldPriority.Peripheral).ToList();

        Assert.Equal(2, peripheral.Count);
        Assert.Contains(peripheral, f => f.Slug == "space-technology");
        Assert.Contains(peripheral, f => f.Slug == "xr");
    }

    [Fact]
    public void Katalog_MemuatKeduaBidangBaruDariADR010()
    {
        // Edge AI dan Data Engineering adalah dua bidang yang audit temukan
        // tidak punya rumah sama sekali. Kalau salah satunya hilang lagi,
        // ini yang memerah.
        Assert.Contains(FieldCatalog.All, f => f.Slug == "edge-ai");
        Assert.Contains(FieldCatalog.All, f => f.Slug == "data-engineering");
    }

    [Fact]
    public void Katalog_MemakaiNamaXRBukanARVR()
    {
        var xr = Assert.Single(FieldCatalog.All, f => f.Slug == "xr");

        Assert.Equal("XR (AR/VR/MR)", xr.Name);
        Assert.DoesNotContain(FieldCatalog.All, f => f.Name == "AR/VR");
    }

    [Fact]
    public void Katalog_MemakaiNamaCloudYangLebarBukanYangSempit()
    {
        // ADR-010 menolak penyempitan jadi "Cloud" karena Docker, DevOps, dan
        // Platform Engineering kehilangan rumah kalau namanya dipersempit.
        Assert.Contains(FieldCatalog.All, f => f.Name == "Cloud & Infrastructure");
        Assert.DoesNotContain(FieldCatalog.All, f => f.Name == "Cloud");
    }

    [Fact]
    public void Katalog_SlugDanUrutanTampilnyaUnik()
    {
        // Keduanya dipakai sebagai indeks unik di basis data. Kalau bentrok di
        // katalog, migrasinya yang akan gagal - jauh dari sini dan dengan pesan
        // yang jauh lebih membingungkan.
        Assert.Equal(FieldCatalog.All.Count, FieldCatalog.All.Select(f => f.Slug).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(FieldCatalog.All.Count, FieldCatalog.All.Select(f => f.DisplayOrder).Distinct().Count());
        Assert.Equal(FieldCatalog.All.Count, FieldCatalog.All.Select(f => f.Id).Distinct().Count());
    }

    [Fact]
    public void Katalog_IdnyaTetapAntarPemanggilan()
    {
        // HasData menuntut nilai yang sama persis tiap kali model dibangun.
        // Id yang dibangkitkan acak akan membuat EF melahirkan migrasi baru
        // setiap build - dan itu baru ketahuan jauh belakangan.
        var first = FieldCatalog.All.Select(f => f.Id).ToList();
        var second = FieldCatalog.All.Select(f => f.Id).ToList();

        Assert.Equal(first, second);
    }

    [Fact]
    public void Katalog_UrutanTampilnyaBerurutDariSatu()
    {
        var orders = FieldCatalog.All.Select(f => f.DisplayOrder).OrderBy(o => o).ToList();

        Assert.Equal(Enumerable.Range(1, FieldCatalog.ExpectedCount), orders);
    }
}
