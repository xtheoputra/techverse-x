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
/// <item><b>Tidak ada input selain <c>sha</c> dan <c>slug</c>.</b> Isinya datang dari
/// berkas di <c>main</c>, yang sudah dibaca di PR. Input ketiga yang membawa teks
/// membuka jalan memasang isi yang tak pernah dibaca siapa pun.</item>
/// <item><b>Hanya dari <c>main</c>, dan sebelum rahasia disentuh.</b> Cabang atau PR
/// membawa berkas yang belum ditinjau, sementara environment <c>produksi</c> memegang
/// rahasia Neon.</item>
/// </list>
/// </remarks>
public sealed class IsiWorkflowGuardTests
{
    private static readonly string[] InputYangDiizinkan = ["sha", "slug"];

    [Fact]
    public void Workflow_isi_hanya_punya_input_sha_dan_slug()
    {
        var jalur = WorkflowYaml.JalurWorkflow("isi.yml");
        var dideklarasikan = WorkflowYaml.InputKeys(File.ReadAllLines(jalur), "isi.yml");

        Assert.True(
            dideklarasikan.SetEquals(InputYangDiizinkan),
            "workflow_dispatch.inputs harus TEPAT { sha, slug }. Input lain — terutama yang membawa isi atau menamai orang — "
            + "membuka jalan memasang teks yang tak pernah dibaca di PR (ADR-027). "
            + $"Ditemukan: [{string.Join(", ", dideklarasikan)}].");

        var dipakai = WorkflowYaml.InputRefs(File.ReadAllText(jalur));
        Assert.True(
            dipakai.IsSubsetOf(InputYangDiizinkan),
            $"hanya inputs.sha & inputs.slug yang boleh dirujuk. Ditemukan: [{string.Join(", ", dipakai)}].");
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
