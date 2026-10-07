namespace TechVerseX.TechnologyService.Tests.Workflows;

/// <summary>
/// Penjaga ADR-027 atas jalan isi sungguhan: <c>.github/workflows/isi.yml</c> dan
/// pemasangnya <c>database/isi/pasang.mjs</c>.
/// </summary>
/// <remarks>
/// 🔴 <b>Kenapa penjaga ini ada.</b> Jalan isi menulis ke produksi dari berkas, dan
/// ada tiga hal yang membuatnya berbahaya kalau melonggar — semuanya aturan yang
/// hari ini cuma ada di komentar, di repo yang "punya sejarah panjang soal aturan
/// yang dinyatakan tapi tidak dijaga" (ADR-021):
/// <list type="number">
/// <item><b>Pemasang tak pernah menaikkan ke <c>tinjau</c>.</b> <c>tinjau</c> satu-satunya
/// klaim kepercayaan produk ini, dan nama pemeriksanya harus datang dari
/// <c>github.actor</c> di <c>tinjau.yml</c>. Kalau pemasang isi bisa memanggil
/// <c>/tinjau</c>, "sudah diperiksa manusia" bisa lahir dari satu klik pemasangan.</item>
/// <item><b>Tidak ada input yang bisa membawa teks selain <c>sha</c> dan <c>slug</c>.</b>
/// Isinya datang dari berkas di <c>main</c>, yang sudah dibaca di PR. Input yang membawa
/// teks membuka jalan memasang isi yang tak pernah dibaca siapa pun. Satu-satunya input
/// tambahan, <c>ganti</c> (ADR-028 Tahap 2), adalah sakelar <b>boolean</b>: ia memilih
/// cara memasang berkas yang sama, tak pernah membawa isi.</item>
/// <item><b>Hanya dari <c>main</c>, dan sebelum rahasia disentuh.</b> Cabang atau PR
/// membawa berkas yang belum ditinjau, sementara environment <c>produksi</c> memegang
/// rahasia Neon.</item>
/// </list>
/// </remarks>
public sealed class IsiWorkflowGuardTests
{
    private static readonly string[] InputYangDiizinkan = ["sha", "slug", "ganti"];

    [Fact]
    public void Workflow_isi_hanya_punya_input_sha_slug_dan_sakelar_ganti()
    {
        var jalur = WorkflowYaml.JalurWorkflow("isi.yml");
        var dideklarasikan = WorkflowYaml.InputKeys(File.ReadAllLines(jalur), "isi.yml");

        Assert.True(
            dideklarasikan.SetEquals(InputYangDiizinkan),
            "workflow_dispatch.inputs harus TEPAT { sha, slug, ganti }. Input lain — terutama yang membawa isi atau menamai orang — "
            + "membuka jalan memasang teks yang tak pernah dibaca di PR (ADR-027). "
            + $"Ditemukan: [{string.Join(", ", dideklarasikan)}].");

        var dipakai = WorkflowYaml.InputRefs(File.ReadAllText(jalur));
        Assert.True(
            dipakai.IsSubsetOf(InputYangDiizinkan),
            $"hanya inputs.sha, inputs.slug & inputs.ganti yang boleh dirujuk. Ditemukan: [{string.Join(", ", dipakai)}].");
    }

    /// <summary>
    /// <c>ganti</c> boleh ada <b>hanya selama ia boolean</b>. Alasan input ketiga ditolak
    /// (ia bisa membawa teks yang tak pernah dibaca di PR) tidak berlaku pada <c>true</c>/<c>false</c>,
    /// dan menjadi berlaku lagi begitu tipenya <c>string</c>.
    /// </summary>
    [Fact]
    public void Sakelar_ganti_bertipe_boolean_dan_tidak_menjadi_argumen_bebas()
    {
        var jalur = WorkflowYaml.JalurWorkflow("isi.yml");
        var baris = File.ReadAllLines(jalur);

        Assert.Equal("boolean", WorkflowYaml.InputType(baris, "ganti"));
        Assert.Equal("string", WorkflowYaml.InputType(baris, "sha"));
        Assert.Equal("string", WorkflowYaml.InputType(baris, "slug"));

        // Dibandingkan dengan "true" PERSIS di shell, dan pemasang dipanggil dengan
        // `--ganti` yang ditulis di berkas — bukan dengan nilai yang datang dari input.
        var teks = File.ReadAllText(jalur);
        Assert.Contains("\"$GANTI\" = \"true\"", teks, StringComparison.Ordinal);
        Assert.Contains("pasang.mjs --ganti \"$SLUG\"", teks, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(".github/workflows/isi.yml")]
    [InlineData("database/isi/pasang.mjs")]
    public void Jalan_isi_tidak_menyentuh_tinjau_dan_tidak_menamai_pemeriksa(string nama)
    {
        var teks = File.ReadAllText(WorkflowYaml.Jalur(nama.Split('/')));

        // Tiga penanda, tiga jalan yang sama: memanggil endpoint tinjau, mengisi nama
        // pemeriksa, atau memakai variabel pemeriksa milik tinjau.yml. Komentar dan
        // pesan sengaja menulis "tinjau" TANPA garis miring — hanya jalurnya yang dilarang.
        Assert.False(
            teks.Contains("/tinjau", StringComparison.Ordinal),
            $"{nama} menyebut '/tinjau'. Pemasang isi paling jauh membawa topik ke draf; naik ke tinjau hanya lewat tinjau.yml (ADR-021 §2, ADR-027).");
        Assert.False(
            teks.Contains("reviewer", StringComparison.OrdinalIgnoreCase),
            $"{nama} menyebut 'reviewer'. Nama pemeriksa hanya boleh lahir di tinjau.yml, dari github.actor (ADR-021 §2).");
        Assert.False(
            teks.Contains("PEMERIKSA", StringComparison.Ordinal),
            $"{nama} memakai PEMERIKSA. Variabel itu milik tinjau.yml (ADR-021 §2).");
    }

    /// <summary>
    /// <c>ISI_DIR</c> membuat pemasang membaca berkas isi dari folder LAIN — itu alat gerbang uji
    /// (<c>uji-media.mjs</c>), bukan sesuatu yang jalan produksi boleh tahu. Isi yang dipasang ke
    /// Neon harus berkas di <c>isi/</c> pada <c>main</c>, yang sudah dibaca di PR (ADR-027).
    /// </summary>
    [Fact]
    public void Workflow_isi_tidak_menyebut_ISI_DIR_jadi_isinya_selalu_berkas_isi_di_main()
    {
        var teks = File.ReadAllText(WorkflowYaml.JalurWorkflow("isi.yml"));

        Assert.False(
            teks.Contains("ISI_DIR", StringComparison.Ordinal),
            "isi.yml menyebut ISI_DIR. Variabel itu hanya untuk gerbang uji; di jalan produksi ia akan membuka jalan memasang isi dari folder yang belum dibaca siapa pun (ADR-027).");
    }

    [Fact]
    public void Workflow_isi_hanya_berjalan_dari_main_sebelum_menyentuh_rahasia()
    {
        var teks = File.ReadAllText(WorkflowYaml.JalurWorkflow("isi.yml"));

        var periksaMain = teks.IndexOf("refs/heads/main", StringComparison.Ordinal);
        var rahasia = teks.IndexOf("secrets.NEON_DATABASE_URL", StringComparison.Ordinal);

        Assert.True(periksaMain >= 0, "isi.yml harus memeriksa ref == refs/heads/main (ADR-027): isi yang belum ter-merge belum dibaca siapa pun.");
        Assert.True(rahasia >= 0, "isi.yml diharapkan memakai secrets.NEON_DATABASE_URL; kalau tidak lagi, penjaga ini perlu ditinjau ulang.");
        Assert.True(
            periksaMain < rahasia,
            "pemeriksaan 'hanya dari main' harus mendahului pemakaian secrets.NEON_DATABASE_URL — kalau urutannya terbalik, kode cabang sudah memegang rahasia.");
    }
}
