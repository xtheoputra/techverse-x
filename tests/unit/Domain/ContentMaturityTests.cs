using TechVerseX.TechnologyService.Domain;

namespace TechVerseX.TechnologyService.Tests.Domain;

/// <summary>
/// Penjaga aturan kematangan konten ADR-012.
/// </summary>
/// <remarks>
/// Aturan yang diuji di sini semuanya berbentuk sama: <b>tidak ada jalan
/// menandai halaman "sudah diperiksa manusia" tanpa manusianya</b>, dan
/// <b>pemeriksaan tidak bertahan melewati perubahan isi</b>. Keduanya ditegakkan
/// tipe dan urutan pemanggilan, bukan komentar.
/// </remarks>
public sealed class ContentMaturityTests
{
    private static readonly Guid AnyField = FieldCatalog.All[0].Id;

    private static Technology NewTechnology() =>
        Technology.Create("Model Context Protocol", "Protokol tool untuk agen.", AnyField);

    [Fact]
    public void MarkReviewed_MenolakHalamanYangMasihKurasi()
    {
        // Tidak ada yang bisa diperiksa dari halaman yang isinya baru daftar
        // tautan. Naik harus lewat MarkDrafted dulu.
        var technology = NewTechnology();

        var error = Assert.Throws<InvalidOperationException>(() => technology.MarkReviewed("pemilik"));
        Assert.Contains("kurasi", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void MarkReviewed_MenuntutNamaPemeriksanya()
    {
        var technology = NewTechnology();
        technology.MarkDrafted();

        Assert.Throws<ArgumentException>(() => technology.MarkReviewed("   "));
    }

    [Fact]
    public void MarkReviewed_MencatatSiapaDanKapan()
    {
        var technology = NewTechnology();
        technology.MarkDrafted();
        technology.ClearEvents();

        technology.MarkReviewed("  pemilik  ");

        Assert.Equal(ContentMaturity.HumanReviewed, technology.Maturity);
        Assert.Equal("pemilik", technology.ReviewedBy);
        Assert.NotNull(technology.ReviewedAt);
        Assert.Equal(nameof(TechnologyReviewed), Assert.Single(technology.Events).EventType);
    }

    [Fact]
    public void Update_MenggugurkanPemeriksaanManusia()
    {
        // Ini aturan yang paling mudah diprotes dan paling penting dipertahankan:
        // pemeriksaan berlaku atas TEKS yang diperiksa, bukan atas nama halaman.
        // Kalau teksnya berubah, labelnya tidak boleh ikut bertahan.
        var technology = NewTechnology();
        technology.MarkDrafted();
        technology.MarkReviewed("pemilik");
        technology.ClearEvents();

        technology.Update("Model Context Protocol", "Ringkasan yang sudah diubah.", AnyField);

        Assert.Equal(ContentMaturity.MachineDrafted, technology.Maturity);
        Assert.Null(technology.ReviewedAt);
        Assert.Null(technology.ReviewedBy);
        Assert.Contains(technology.Events, e => e.EventType == nameof(TechnologyReviewExpired));
    }

    [Fact]
    public void Update_TidakMenerbitkanReviewExpiredKalauMemangBelumPernahDiperiksa()
    {
        var technology = NewTechnology();
        technology.ClearEvents();

        technology.Update("Nama Baru", "Ringkasan baru.", AnyField);

        Assert.DoesNotContain(technology.Events, e => e.EventType == nameof(TechnologyReviewExpired));
        Assert.Equal(ContentMaturity.Curated, technology.Maturity);
    }

    [Fact]
    public void MarkDrafted_MenolakMenurunkanHalamanYangSudahDiperiksa()
    {
        // Menurunkan tingkat harus punya sebab yang tercatat (perubahan isi),
        // bukan dilakukan diam-diam dengan menandainya draf lagi.
        var technology = NewTechnology();
        technology.MarkDrafted();
        technology.MarkReviewed("pemilik");

        var error = Assert.Throws<InvalidOperationException>(technology.MarkDrafted);
        Assert.Contains("Update", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void MarkDrafted_DipanggilDuaKaliTidakMengubahApaPun()
    {
        var technology = NewTechnology();
        technology.MarkDrafted();
        var firstUpdate = technology.UpdatedAt;

        technology.MarkDrafted();

        Assert.Equal(ContentMaturity.MachineDrafted, technology.Maturity);
        Assert.Equal(firstUpdate, technology.UpdatedAt);
    }

    [Fact]
    public void KematanganDanStatusAdalahDuaSumbuYangBerbeda()
    {
        // Penjaga langsung atas ADR-015 bagian 1. Kalau suatu saat kedua enum
        // digabung, uji ini tidak akan bisa dikompilasi lagi - dan itu memang
        // maksudnya.
        var technology = NewTechnology();
        technology.MarkDrafted();
        technology.Publish();

        Assert.Equal(TechnologyStatus.Published, technology.Status);
        Assert.Equal(ContentMaturity.MachineDrafted, technology.Maturity);
    }
}
