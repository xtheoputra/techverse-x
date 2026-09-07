using Microsoft.EntityFrameworkCore;
using TechVerseX.Contracts.Common;
using TechVerseX.Contracts.Technology;
using TechVerseX.TechnologyService.Infrastructure.Persistence;

namespace TechVerseX.TechnologyService.Features.SearchTechnology;

public sealed record SearchTechnologyQuery(string? Term, string? Category, int Page = 1, int PageSize = 20)
{
    public const int MaxPageSize = 100;

    public int NormalizedPage => Page < 1 ? 1 : Page;

    public int NormalizedPageSize => PageSize switch
    {
        < 1 => 20,
        > MaxPageSize => MaxPageSize,
        _ => PageSize,
    };
}

/// <summary>
/// Pencarian V1 — masih LIKE biasa di PostgreSQL.
/// </summary>
/// <remarks>
/// KERANGKA.md 4.6 (Search Architecture) menetapkan tangga V1 PostgreSQL FTS →
/// V2 OpenSearch → V3 hybrid. Ini bahkan belum V1 penuh: FTS memerlukan kolom
/// tsvector, dan itu bagian dari skema yang masih Issue #20. Sengaja dibiarkan
/// sederhana supaya tidak menebak skema yang belum diputuskan.
/// </remarks>
public sealed class SearchTechnologyHandler(TechnologyDbContext db)
{
    public async Task<PagedResponse<TechnologySummaryResponse>> HandleAsync(
        SearchTechnologyQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var source = db.Technologies.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Term))
        {
            var term = $"%{query.Term.Trim()}%";
            source = source.Where(t => EF.Functions.ILike(t.Name, term) || EF.Functions.ILike(t.Summary, term));
        }

        if (!string.IsNullOrWhiteSpace(query.Category))
        {
            var category = query.Category.Trim();
            source = source.Where(t => t.Category == category);
        }

        var total = await source.CountAsync(cancellationToken).ConfigureAwait(false);

        var items = await source
            .OrderBy(t => t.Name)
            .Skip((query.NormalizedPage - 1) * query.NormalizedPageSize)
            .Take(query.NormalizedPageSize)
            .Select(t => new TechnologySummaryResponse(t.Id, t.Slug, t.Name, t.Summary, t.Category))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PagedResponse<TechnologySummaryResponse>(items, query.NormalizedPage, query.NormalizedPageSize, total);
    }
}
