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

        var dideklarasikan = InputKeys(lines);
        Assert.True(
            dideklarasikan.SetEquals(InputYangDiizinkan),
            "workflow_dispatch.inputs harus TEPAT { sha, slug }. Input lain — terutama yang menamai pemeriksa — "
            + $"mengembalikan nama 'atas nama' yang ADR-021 §2 buang. Ditemukan: [{string.Join(", ", dideklarasikan)}].");

        // Pertahanan kedua: tidak ada ${{ inputs.X }} / github.event.inputs.X selain
        // sha & slug yang terpakai di mana pun — menjaga reviewer tak pernah dirangkai
        // dari sebuah input walau deklarasinya entah bagaimana lolos.
        var dipakai = InputRefs(File.ReadAllText(JalurWorkflow()));
        Assert.True(
            dipakai.IsSubsetOf(InputYangDiizinkan),
            $"hanya inputs.sha & inputs.slug yang boleh dirujuk. Ditemukan: [{string.Join(", ", dipakai)}].");
    }

    /// <summary>Kunci-kunci langsung di bawah <c>workflow_dispatch.inputs</c>.</summary>
    private static HashSet<string> InputKeys(string[] lines)
    {
        var keys = new HashSet<string>(StringComparer.Ordinal);

        var inputsIdx = -1;
        for (var i = 0; i < lines.Length; i++)
        {
            if (lines[i].Trim() == "inputs:")
            {
                inputsIdx = i;
                break;
            }
        }

        Assert.True(inputsIdx >= 0, "tinjau.yml harus punya blok workflow_dispatch.inputs.");

        var inputsIndent = Indent(lines[inputsIdx]);
        var keyIndent = -1;

        for (var i = inputsIdx + 1; i < lines.Length; i++)
        {
            var line = lines[i];
            if (line.Trim().Length == 0)
            {
                continue;
            }

            var indent = Indent(line);
            if (indent <= inputsIndent)
            {
                break; // keluar dari blok inputs
            }

            var trimmed = line.TrimStart(' ');
            if (trimmed.StartsWith('#'))
            {
                continue;
            }

            if (keyIndent == -1)
            {
                keyIndent = indent;
            }

            if (indent != keyIndent)
            {
                continue; // baris di dalam definisi sebuah input (description, type, ...)
            }

            var colon = trimmed.IndexOf(':');
            if (colon > 0)
            {
                var key = trimmed[..colon];
                if (!key.Contains(' '))
                {
                    keys.Add(key);
                }
            }
        }

        return keys;
    }

    /// <summary>Semua nama input yang dirujuk <c>inputs.X</c> di mana pun di berkas.</summary>
    private static HashSet<string> InputRefs(string teks)
    {
        var refs = new HashSet<string>(StringComparer.Ordinal);
        const string penanda = "inputs.";

        var idx = teks.IndexOf(penanda, StringComparison.Ordinal);
        while (idx >= 0)
        {
            var j = idx + penanda.Length;
            var start = j;
            while (j < teks.Length && (char.IsLetterOrDigit(teks[j]) || teks[j] == '_' || teks[j] == '-'))
            {
                j++;
            }

            if (j > start)
            {
                refs.Add(teks[start..j]);
            }

            idx = teks.IndexOf(penanda, j, StringComparison.Ordinal);
        }

        return refs;
    }

    private static int Indent(string line) => line.Length - line.TrimStart(' ').Length;

    /// <summary>
    /// Menyusuri ke atas dari direktori keluaran uji sampai menemukan
    /// <c>.github/workflows/tinjau.yml</c>. Gagal keras kalau tidak ketemu — penjaga
    /// yang diam-diam tidak membaca berkasnya sama saja dengan tidak ada.
    /// </summary>
    private static string JalurWorkflow()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var kandidat = Path.Combine(dir.FullName, ".github", "workflows", "tinjau.yml");
            if (File.Exists(kandidat))
            {
                return kandidat;
            }

            dir = dir.Parent;
        }

        throw new FileNotFoundException(
            "Tidak menemukan .github/workflows/tinjau.yml dengan menyusuri ke atas dari " + AppContext.BaseDirectory);
    }
}
