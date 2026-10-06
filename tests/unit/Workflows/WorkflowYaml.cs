namespace TechVerseX.TechnologyService.Tests.Workflows;

/// <summary>
/// Pembacaan berkas workflow yang dipakai bersama penjaga-penjaganya
/// (<see cref="TinjauWorkflowGuardTests"/>, <see cref="IsiWorkflowGuardTests"/>).
/// </summary>
/// <remarks>
/// Satu implementasi, bukan salinan per penjaga: aturan "hanya input ini" yang dibaca
/// dua penjaga dengan dua pembaca YAML berbeda bisa menyimpang tanpa ada yang merah.
/// Pembacanya sengaja sederhana (indentasi, bukan parser YAML) — yang dijaga adalah
/// <em>kunci-kunci langsung</em> di bawah <c>inputs:</c>, dan itu tak butuh lebih.
/// </remarks>
internal static class WorkflowYaml
{
    /// <summary>
    /// Menyusuri ke atas dari direktori keluaran uji sampai menemukan berkasnya.
    /// Gagal keras kalau tidak ketemu — penjaga yang diam-diam tidak membaca berkasnya
    /// sama saja dengan tidak ada.
    /// </summary>
    public static string Jalur(params string[] bagian)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var kandidat = Path.Combine([dir.FullName, .. bagian]);
            if (File.Exists(kandidat))
            {
                return kandidat;
            }

            dir = dir.Parent;
        }

        throw new FileNotFoundException(
            $"Tidak menemukan {string.Join('/', bagian)} dengan menyusuri ke atas dari {AppContext.BaseDirectory}");
    }

    public static string JalurWorkflow(string namaBerkas) => Jalur(".github", "workflows", namaBerkas);

    /// <summary>Kunci-kunci langsung di bawah <c>workflow_dispatch.inputs</c>.</summary>
    public static HashSet<string> InputKeys(string[] lines, string namaBerkas)
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

        Assert.True(inputsIdx >= 0, $"{namaBerkas} harus punya blok workflow_dispatch.inputs.");

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
    public static HashSet<string> InputRefs(string teks)
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
}
