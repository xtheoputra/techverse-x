using TechVerseX.TechnologyService.Domain;

namespace TechVerseX.TechnologyService.Tests.Domain;

/// <summary>
/// Keputusan pemilik 2026-10-06 (#79, ADR-012 Pembaruan 2026-10-06): pada topik yang
/// sudah <c>tinjau</c>, <b>mengganti</b> teks yang sudah diperiksa menggugurkan
/// pemeriksaan seperti <see cref="Technology.Update"/>; <b>menambah</b> dan
/// <b>membuang</b> bagian tidak (dijaga <c>BagianIsiHalamanTests</c>).
/// </summary>
/// <remarks>
/// "Mengganti" berarti isi yang tersimpan diganti dengan isi yang BERBEDA sesudah
/// dinormalisasi (dipangkas). Mengulang isi yang sama bukan perubahan teks: pemeriksa
/// membaca teks yang persis sama, dan permintaan ulang yang idempoten tidak boleh
/// membuang kerjanya.
/// </remarks>
public sealed class GantiTeksMenurunkanTinjauTests
{
    private static readonly Guid AnyField = FieldCatalog.All[0].Id;

    private static Technology Ditinjau()
    {
        var technology = Technology.Create("AI Agents", "Dari AI yang menjawab ke AI yang mengerjakan.", AnyField)
            .IsiKelimaBagian();
        technology.MarkDrafted();
        technology.MarkReviewed("pemilik");
        technology.ClearEvents();
        return technology;
    }

    private static void AssertDigugurkan(Technology technology)
    {
        Assert.Equal(ContentMaturity.MachineDrafted, technology.Maturity);
        Assert.Null(technology.ReviewedAt);
        Assert.Null(technology.ReviewedBy);
        Assert.Single(technology.Events, e => e.EventType == nameof(TechnologyReviewExpired));
    }

    private static void AssertTetapDitinjau(Technology technology)
    {
        Assert.Equal(ContentMaturity.HumanReviewed, technology.Maturity);
        Assert.Equal("pemilik", technology.ReviewedBy);
        Assert.NotNull(technology.ReviewedAt);
        Assert.DoesNotContain(technology.Events, e => e.EventType == nameof(TechnologyReviewExpired));
    }

    // ---- Prasyarat (langkah 0) --------------------------------------------

    [Fact]
    public void SetPrerequisite_MenggantiJudul_MenggugurkanPemeriksaan()
    {
        var technology = Ditinjau();

        technology.SetPrerequisite("Prasyarat yang berbeda", "Python dasar dan satu panggilan HTTP.");

        AssertDigugurkan(technology);
        Assert.Equal("Prasyarat yang berbeda", technology.Roadmap[0].Title);
    }

    [Fact]
    public void SetPrerequisite_MenggantiUraian_MenggugurkanPemeriksaan()
    {
        var technology = Ditinjau();

        technology.SetPrerequisite("Prasyarat", "Teks yang sama sekali berbeda.");

        AssertDigugurkan(technology);
        Assert.Equal("Teks yang sama sekali berbeda.", technology.Roadmap[0].Description);
    }

    [Fact]
    public void SetPrerequisite_IsiSama_TIDAKMenggugurkanPemeriksaan()
    {
        var technology = Ditinjau();

        technology.SetPrerequisite("Prasyarat", "Python dasar dan satu panggilan HTTP.");

        AssertTetapDitinjau(technology);
    }

    [Fact]
    public void SetPrerequisite_IsiSamaHanyaBedaSpasiTepi_TIDAKMenggugurkanPemeriksaan()
    {
        // Dibandingkan sesudah dipangkas, seperti yang disimpan RoadmapStep.Create:
        // spasi di tepi bukan perubahan teks yang dibaca pemeriksa.
        var technology = Ditinjau();

        technology.SetPrerequisite("  Prasyarat ", "\tPython dasar dan satu panggilan HTTP.\n");

        AssertTetapDitinjau(technology);
    }

    [Fact]
    public void SetPrerequisite_PadaTopikBelumDiperiksa_MenggantiTeksTanpaEventKedaluwarsa()
    {
        var technology = Technology.Create("AI Agents", "Ringkasan.", AnyField).IsiKelimaBagian();
        technology.MarkDrafted();
        technology.ClearEvents();

        technology.SetPrerequisite("Prasyarat", "Teks yang berbeda.");

        Assert.Equal(ContentMaturity.MachineDrafted, technology.Maturity);
        Assert.DoesNotContain(technology.Events, e => e.EventType == nameof(TechnologyReviewExpired));
    }

    // ---- Catatan alat -------------------------------------------------------

    [Fact]
    public void AttachTool_MenggantiCatatan_MenggugurkanPemeriksaan()
    {
        var technology = Ditinjau();

        technology.AttachTool(TemplateLengkap.AlatContoh, "Catatan yang diubah.");

        AssertDigugurkan(technology);
        Assert.Equal("Catatan yang diubah.", Assert.Single(technology.Tools).Note);
    }

    [Fact]
    public void AttachTool_MenghapusCatatan_MenggugurkanPemeriksaan()
    {
        // Catatan yang tadinya ada lalu dikosongkan juga teks yang berubah.
        var technology = Ditinjau();

        technology.AttachTool(TemplateLengkap.AlatContoh, note: null);

        AssertDigugurkan(technology);
        Assert.Null(Assert.Single(technology.Tools).Note);
    }

    [Fact]
    public void AttachTool_CatatanSama_TIDAKMenggugurkanPemeriksaan()
    {
        var technology = Ditinjau();

        technology.AttachTool(TemplateLengkap.AlatContoh, "Dipakai di langkah pertama.");

        AssertTetapDitinjau(technology);
    }

    [Fact]
    public void AttachTool_AlatBaru_TIDAKMenggugurkanPemeriksaan()
    {
        // MENAMBAH alat adalah sisi "tambah" dari aturan ini, bukan "mengganti".
        var technology = Ditinjau();

        technology.AttachTool(Guid.CreateVersion7(), "Alat kedua.");

        AssertTetapDitinjau(technology);
        Assert.Equal(2, technology.Tools.Count);
    }

    [Fact]
    public void AttachTool_PadaTopikBelumDiperiksa_MenggantiCatatanTanpaEventKedaluwarsa()
    {
        var technology = Technology.Create("AI Agents", "Ringkasan.", AnyField).IsiKelimaBagian();
        technology.ClearEvents();

        technology.AttachTool(TemplateLengkap.AlatContoh, "Catatan yang diubah.");

        Assert.DoesNotContain(technology.Events, e => e.EventType == nameof(TechnologyReviewExpired));
        Assert.Equal(ContentMaturity.Curated, technology.Maturity);
    }

    // ---- Sesudah gugur, jalan kembali tetap satu ---------------------------

    [Fact]
    public void SesudahGugur_PemeriksaanBisaDiulangDenganMarkReviewed()
    {
        var technology = Ditinjau();
        technology.SetPrerequisite("Prasyarat", "Teks yang berbeda.");

        technology.MarkReviewed("pemeriksa kedua");

        Assert.Equal(ContentMaturity.HumanReviewed, technology.Maturity);
        Assert.Equal("pemeriksa kedua", technology.ReviewedBy);
    }
}
