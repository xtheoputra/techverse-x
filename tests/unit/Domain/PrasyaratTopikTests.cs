using TechVerseX.TechnologyService.Domain;

namespace TechVerseX.TechnologyService.Tests.Domain;

/// <summary>
/// Penjaga <see cref="Technology.RequireTopic"/> — satu-satunya jalan menulis sisi
/// knowledge graph (ADR-023).
/// </summary>
/// <remarks>
/// Yang dijaga di sini aturan yang hidup di DOMAIN, bukan di basis data: arah
/// sisinya, idempotensi, penolakan sisi ke diri sendiri dengan nama medan yang
/// dimengerti pemanggil, penolakan dua topik yang saling mensyaratkan, dan dua
/// hal yang TIDAK boleh dilakukan relasi — menggugurkan pemeriksaan manusia dan
/// ikut dihitung sebagai bagian template.
/// <para>
/// ⚠️ Lapis kedua (kunci asing di kedua ujung, CHECK diri sendiri, CHECK jenis)
/// dijaga <c>PrasyaratTopikPersistenceTests</c> di uji integrasi, sebab hanya
/// PostgreSQL sungguhan yang bisa membuktikannya.
/// </para>
/// </remarks>
public sealed class PrasyaratTopikTests
{
    private static readonly Guid AnyField = FieldCatalog.All[0].Id;

    private static Technology Topik(string name) =>
        Technology.Create(name, "Ringkasan untuk uji.", AnyField);

    [Fact]
    public void RequireTopic_menyimpan_satu_sisi_Requires_dari_topik_ini()
    {
        var mcp = Topik("Model Context Protocol");
        var toolUse = Topik("Tool Use");
        var bagianSebelum = mcp.MissingSections.ToList();

        mcp.RequireTopic(toolUse.Id, topicRequires: []);

        // Arah yang dijaga: ASAL membutuhkan TUJUAN. Tertukar, dan setiap halaman
        // menyuruh pembaca mempelajari topiknya sebelum prasyaratnya.
        var sisi = Assert.Single(mcp.Relationships);
        Assert.Equal(mcp.Id, sisi.FromTechnologyId);
        Assert.Equal(toolUse.Id, sisi.ToTechnologyId);
        Assert.Equal(RelationshipKind.Requires, sisi.Kind);

        Assert.Equal(bagianSebelum, mcp.MissingSections);
    }

    [Fact]
    public void RequireTopic_dua_kali_tetap_satu_sisi()
    {
        // Hanya jumlahnya. UpdatedAt sengaja tidak diperiksa: dua panggilan dalam
        // satu tik jam membuat uji waktu seperti itu hijau karena kebetulan.
        var mcp = Topik("Model Context Protocol");
        var toolUse = Topik("Tool Use");

        mcp.RequireTopic(toolUse.Id, topicRequires: []);
        mcp.RequireTopic(toolUse.Id, topicRequires: []);

        Assert.Single(mcp.Relationships);
    }

    [Fact]
    public void RequireTopic_ke_diri_sendiri_ditolak_dengan_medan_topicSlug()
    {
        // TechnologyRelationship.Create juga menolak ini — tapi atas nama
        // "toTechnologyId", nama yang tidak ada di muatan permintaan mana pun.
        // Pemanggil yang menerjemahkan galat ke ValidationProblem butuh nama
        // medan yang benar-benar ia kirim.
        var mcp = Topik("Model Context Protocol");

        var galat = Assert.Throws<ArgumentException>(() => mcp.RequireTopic(mcp.Id, topicRequires: []));

        Assert.Equal("topicSlug", galat.ParamName);
        Assert.Empty(mcp.Relationships);
    }

    [Fact]
    public void RequireTopic_ditolak_bila_topik_tujuan_sudah_mensyaratkan_topik_ini()
    {
        var mcp = Topik("Model Context Protocol");
        var toolUse = Topik("Tool Use");

        // Tool Use (di agregatnya sendiri) sudah membutuhkan MCP. Yang dikirim ke
        // MCP adalah sisi-sisi Requires milik Tool Use — dan di dalamnya ada MCP.
        var galat = Assert.Throws<InvalidOperationException>(
            () => mcp.RequireTopic(toolUse.Id, topicRequires: [mcp.Id]));

        Assert.Contains("saling mensyaratkan", galat.Message, StringComparison.Ordinal);
        Assert.Empty(mcp.Relationships);
    }

    [Fact]
    public void RequireTopic_TIDAK_menggugurkan_pemeriksaan_manusia()
    {
        // Cermin MenambahBagian_TIDAKMenggugurkanPemeriksaanManusia: menautkan
        // halaman yang sudah diperiksa ke topik lain bukan pembatalan pemeriksaan
        // atas TEKSnya. Kalau pendapat soal ini berubah, perubahannya harus
        // disengaja.
        var mcp = Topik("Model Context Protocol").IsiKelimaBagian();
        mcp.MarkDrafted();
        mcp.MarkReviewed("pemilik");

        mcp.RequireTopic(Topik("Tool Use").Id, topicRequires: []);

        Assert.Equal(ContentMaturity.HumanReviewed, mcp.Maturity);
        Assert.Equal("pemilik", mcp.ReviewedBy);
    }

    [Fact]
    public void RelationshipKind_V1_hanya_Requires()
    {
        // Menambah jenis menuntut ADR, migrasi (CHECK-nya dibangkitkan dari enum
        // ini), dan label tampilan. Uji ini memastikan penambahan itu tidak
        // terjadi sebagai satu baris yang lolos tanpa ketiganya.
        var nama = Enum.GetNames<RelationshipKind>();

        Assert.True(
            nama.SequenceEqual(["Requires"]),
            $"RelationshipKind V1 hanya Requires (ADR-023), sekarang: {string.Join(", ", nama)}. "
            + "Jenis baru butuh ADR, migrasi CHECK ck_technology_relationships_kind, dan label di web.");
    }

    [Fact]
    public void MissingSections_tidak_menuntut_relasi()
    {
        // Relasi bukan bagian template ADR-012. Topik yang kelima bagiannya
        // terisi TANPA satu sisi pun tetap lengkap — kalau tidak, setiap halaman
        // kurasi di tiga belas bidang lain ditandai belum selesai.
        var topik = Topik("Qiskit").IsiKelimaBagian();

        Assert.Empty(topik.Relationships);
        Assert.Empty(topik.MissingSections);
    }
}
