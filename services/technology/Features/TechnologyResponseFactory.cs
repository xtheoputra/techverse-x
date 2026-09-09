using TechVerseX.Contracts.Technology;
using TechVerseX.TechnologyService.Domain;

namespace TechVerseX.TechnologyService.Features;

/// <summary>
/// Satu-satunya tempat sebuah <see cref="Technology"/> berubah jadi
/// <see cref="TechnologyResponse"/>.
/// </summary>
/// <remarks>
/// 🔑 <b>Ia terpusat karena <c>MissingSections</c> tidak boleh dihitung dua kali.</b>
/// Aturan kelengkapan ADR-012 punya satu bagian yang gampang salah disalin —
/// roadmap dianggap terisi hanya kalau ada langkah <em>sesudah</em> langkah 0.
/// Selama pemetaan ini cuma ada di satu berkas, aturan itu tetap tinggal di
/// agregat; begitu ia disalin ke tiap handler, salah satunya akan menulis
/// <c>Count &gt; 0</c> dan diam-diam meluluskan roadmap yang hanya berisi prasyarat.
/// </remarks>
internal static class TechnologyResponseFactory
{
    public static TechnologyResponse From(
        Technology technology,
        Field field,
        IReadOnlyDictionary<Guid, Tool>? toolCatalog = null)
    {
        ArgumentNullException.ThrowIfNull(technology);
        ArgumentNullException.ThrowIfNull(field);

        var catalog = toolCatalog ?? new Dictionary<Guid, Tool>();

        return new TechnologyResponse(
            technology.Id,
            technology.Slug,
            technology.Name,
            technology.Summary,
            field.Slug,
            field.Name,
            technology.Status.ToString(),
            technology.Maturity.ToString(),
            technology.ReviewedAt,
            technology.CreatedAt,
            technology.UpdatedAt,
            [.. technology.Roadmap
                .OrderBy(s => s.Order)
                .Select(s => new RoadmapStepResponse(s.Order, s.IsPrerequisite, s.Title, s.Description))],
            [.. technology.Tools
                .Where(link => catalog.ContainsKey(link.ToolId))
                .Select(link =>
                {
                    var tool = catalog[link.ToolId];
                    return new TechnologyToolResponse(tool.Slug, tool.Name, tool.Summary, tool.Homepage, link.Note);
                })
                .OrderBy(t => t.Name, StringComparer.Ordinal)],
            [.. technology.Projects
                .OrderBy(p => p.CreatedAt)
                .Select(p => new ProjectResponse(p.Id, p.Title, p.Brief))],
            [.. technology.Resources
                .OrderBy(r => r.Type)
                .ThenBy(r => r.CreatedAt)
                .Select(r => new ResourceResponse(r.Id, r.Type.ToString(), r.Title, r.Url))],
            [.. technology.MissingSections]);
    }
}
