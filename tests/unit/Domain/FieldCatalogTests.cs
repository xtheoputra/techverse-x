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
    public void Katalog_BerisiTujuhBelasBidang()
    {
        // Empat belas dari ADR-010 + tiga bidang Deep Tech (ADR-010 Pembaruan 2026-10-07).
        Assert.Equal(FieldCatalog.ExpectedCount, FieldCatalog.All.Count);
        Assert.Equal(17, FieldCatalog.All.Count);
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
    public void Katalog_PunyaEmpatBidangYangBerhentiDiKurasiTautan()
    {
        // Space dan XR dari ADR-010, ditambah Neurotechnology & BCI dan Advanced
        // Materials & Nanotech (Pembaruan 2026-10-07): "Target: kurasi tautan".
        var peripheral = FieldCatalog.All.Where(f => f.Priority == FieldPriority.Peripheral).ToList();

        Assert.Equal(4, peripheral.Count);
        Assert.Contains(peripheral, f => f.Slug == "space-technology");
        Assert.Contains(peripheral, f => f.Slug == "xr");
        Assert.Contains(peripheral, f => f.Slug == "neurotechnology-bci");
        Assert.Contains(peripheral, f => f.Slug == "advanced-materials-nanotech");
    }

    [Fact]
    public void Katalog_PunyaTujuhBidangPrioritasDua_SatuDiantaranyaDariPembaruan()
    {
        var supporting = FieldCatalog.All.Where(f => f.Priority == FieldPriority.Supporting).ToList();

        Assert.Equal(7, supporting.Count);
        Assert.Contains(supporting, f => f.Slug == "advanced-computing-hardware");
    }

    [Fact]
    public void Katalog_MemuatKetigaBidangDeepTechDiBawahXR_DenganNamaDariPemilik()
    {
        // Ditambahkan pemilik 2026-10-07 dan diletakkan di bawah XR: urutan tampil 15-17.
        // Bahwa urutan tampil kini tak sepenuhnya menurut prioritas (prioritas 2 sesudah
        // prioritas 3) dicatat di ADR-010 Pembaruan 2026-10-07 — uji ini menguncinya supaya
        // pergeserannya disengaja.
        var xr = Assert.Single(FieldCatalog.All, f => f.Slug == "xr");

        var komputasi = Assert.Single(FieldCatalog.All, f => f.Slug == "advanced-computing-hardware");
        var neuro = Assert.Single(FieldCatalog.All, f => f.Slug == "neurotechnology-bci");
        var material = Assert.Single(FieldCatalog.All, f => f.Slug == "advanced-materials-nanotech");

        Assert.Equal("Advanced Computing & Hardware", komputasi.Name);
        Assert.Equal("Neurotechnology & BCI", neuro.Name);
        Assert.Equal("Advanced Materials & Nanotech", material.Name);

        Assert.Equal([15, 16, 17], new[] { komputasi.DisplayOrder, neuro.DisplayOrder, material.DisplayOrder });
        Assert.All(new[] { komputasi, neuro, material }, f => Assert.True(f.DisplayOrder > xr.DisplayOrder));
    }

    [Fact]
    public void Katalog_TidakMengklaimSuperkonduktorSuhuKamarSudahAda()
    {
        // 🔴 Ringkasan bidang tercetak di kartu PUBLIK. "Superkonduktor suhu kamar" yang
        // ditulis tanpa catatan terbaca sebagai bahan yang sudah ada — padahal belum ada yang
        // terbukti (klaim LK-99 2023 gugur oleh replikasi independen). Teks asli permintaan
        // pemilik memuat frasa itu telanjang; kata "belum terbukti" ditambahkan dan dijaga di sini.
        var material = Assert.Single(FieldCatalog.All, f => f.Slug == "advanced-materials-nanotech");

        Assert.Contains("suhu kamar", material.Summary, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("belum terbukti", material.Summary, StringComparison.OrdinalIgnoreCase);
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
