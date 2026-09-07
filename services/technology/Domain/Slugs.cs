using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace TechVerseX.TechnologyService.Domain;

/// <summary>
/// Pembuat slug URL. Dipakai <see cref="Technology"/> maupun <see cref="Field"/>.
/// </summary>
/// <remarks>
/// Slug adalah URL kanonik dari ADR-009 (<c>/teknologi/&lt;slug&gt;</c>) — satu-satunya
/// bentuk alamat yang dijanjikan stabil. Karena itu aturannya hidup di satu tempat:
/// dua entitas yang membuat slug dengan cara berbeda berarti dua alamat berbeda
/// untuk nama yang sama.
/// </remarks>
public static partial class Slugs
{
    /// <summary>Mengubah "AI Agents" menjadi "ai-agents".</summary>
    public static string From(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var normalized = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var ch in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(ch);
            }
        }

        var withoutMarks = builder.ToString().Normalize(NormalizationForm.FormC);
        var slug = NonSlugCharacters().Replace(withoutMarks, "-");
        slug = CollapsedDashes().Replace(slug, "-").Trim('-');

        return slug.Length == 0
            ? throw new ArgumentException($"'{value}' tidak menyisakan satu karakter pun yang bisa dipakai sebagai slug.", nameof(value))
            : slug;
    }

    /// <summary>
    /// Varian yang tidak melempar. Dipakai di jalur yang masukannya datang dari
    /// luar: "!!!" adalah permintaan yang keliru (400), bukan galat server (500).
    /// </summary>
    public static bool TryFrom(string? value, out string slug)
    {
        slug = string.Empty;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        try
        {
            slug = From(value);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NonSlugCharacters();

    [GeneratedRegex("-{2,}")]
    private static partial Regex CollapsedDashes();
}
