namespace TechVerseX.TechnologyService.Tests.Workflows;

/// <summary>
/// Penjaga aturan ADR-021 §2 atas <c>.github/workflows/tinjau.yml</c>: nama
/// pemeriksa datang dari <c>github.actor</c>, dan <b>tidak ada masukan untuk
/// menimpanya</b>.
/// </summary>
/// <remarks>
/// 🔴 <b>Kenapa penjaga ini ada.</b> ADR-021 menulisnya sendiri sebagai satu-satunya
/// aturannya yang "belum punya penjaga sama sekali", di repo yang "punya sejarah
/// panjang soal aturan yang dinyatakan tapi tidak dijaga". Nama pemeriksa adalah
/// satu-satunya klaim kepercayaan produk ini; kalau workflow bisa menerima nama yang
/// diketik ("atas nama X"), klaim itu bisa dipalsukan — persis sifat teks-bebas yang
/// ADR-021 §2 buang dengan mengambil nama dari <c>github.actor</c>.
/// <para>
/// 🔑 <b>Invariannya, dan kenapa ia ketat.</b> Workflow hanya boleh mendeklarasikan
/// input <c>sha</c> dan <c>slug</c> — tidak ada yang ketiga. Menambah input apa pun
/// (apalagi yang menamai pemeriksa) menggagalkan uji ini dengan sengaja: ia memaksa
/// pembaca kembali ke ADR-021 §2 sebelum melonggarkan gerbangnya, bukan menyelipkan
/// medan "atas nama" diam-diam.
/// </para>
/// </remarks>
public sealed class TinjauWorkflowGuardTests
{
    private static readonly string[] InputYangDiizinkan = ["sha", "slug"];

    [Fact]
    public void Workflow_tinjau_mengambil_pemeriksa_dari_github_actor()
    {
        var teks = File.ReadAllText(JalurWorkflow());

        // PEMERIKSA dibaca langkah "Naikkan ke tinjau" sebagai badan permintaan.
        // Spasi dibuang supaya uji ini tidak bergantung pada perataan YAML.
        var tanpaSpasi = teks.Replace(" ", string.Empty, StringComparison.Ordinal);
        Assert.True(
            tanpaSpasi.Contains("PEMERIKSA:${{github.actor}}", StringComparison.Ordinal),
            "tinjau.yml harus mengikat PEMERIKSA ke ${{ github.actor }} — nama pemeriksa tidak boleh dari sumber lain (ADR-021 §2).");

        // Dan badan permintaannya memakai $PEMERIKSA itu, bukan nilai lain.
        Assert.True(
            teks.Contains("\"reviewer\"", StringComparison.Ordinal) && teks.Contains("$PEMERIKSA", StringComparison.Ordinal),
            "badan /tinjau harus mengisi reviewer dari $PEMERIKSA (ADR-021 §2).");
    }

    [Fact]
    public void Workflow_tinjau_tidak_punya_masukan_untuk_menimpa_pemeriksa()
    {
        var lines = File.ReadAllLines(JalurWorkflow());

        var dideklarasikan = WorkflowYaml.InputKeys(lines, "tinjau.yml");
        Assert.True(
            dideklarasikan.SetEquals(InputYangDiizinkan),
            "workflow_dispatch.inputs harus TEPAT { sha, slug }. Input lain — terutama yang menamai pemeriksa — "
            + $"mengembalikan nama 'atas nama' yang ADR-021 §2 buang. Ditemukan: [{string.Join(", ", dideklarasikan)}].");

        // Pertahanan kedua: tidak ada ${{ inputs.X }} / github.event.inputs.X selain
        // sha & slug yang terpakai di mana pun — menjaga reviewer tak pernah dirangkai
        // dari sebuah input walau deklarasinya entah bagaimana lolos.
        var dipakai = WorkflowYaml.InputRefs(File.ReadAllText(JalurWorkflow()));
        Assert.True(
            dipakai.IsSubsetOf(InputYangDiizinkan),
            $"hanya inputs.sha & inputs.slug yang boleh dirujuk. Ditemukan: [{string.Join(", ", dipakai)}].");
    }

    private static string JalurWorkflow() => WorkflowYaml.JalurWorkflow("tinjau.yml");
}
