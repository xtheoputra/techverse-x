using Microsoft.EntityFrameworkCore;
using TechVerseX.Contracts.Common;
using TechVerseX.Contracts.Technology;
using TechVerseX.TechnologyService.Domain;
using TechVerseX.TechnologyService.Infrastructure.Persistence;

namespace TechVerseX.TechnologyService.Features.SearchTechnology;

public sealed record SearchTechnologyQuery(string? Term, string? Field, int Page = 1, int PageSize = 20)
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
/// tsvector, dan itu dijadwalkan Bulan 3 di docs/RENCANA-V1.md — bersama
/// keputusan ADR-015 bahwa pencarian hibrida dijawab `tsvector` PostgreSQL,
/// bukan basis data kedua.
/// </remarks>
public sealed class SearchTechnologyHandler(TechnologyDbContext db)
{
    public async Task<PagedResponse<TechnologySummaryResponse>> HandleAsync(
        SearchTechnologyQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var source = db.Technologies
            .AsNoTracking()
            .Join(
                db.Fields.AsNoTracking(),
                t => t.FieldId,
                f => f.Id,
                (t, f) => new { Technology = t, Field = f });

        if (!string.IsNullOrWhiteSpace(query.Term))
        {
            var term = $"%{query.Term.Trim()}%";
            source = source.Where(x =>
                EF.Functions.ILike(x.Technology.Name, term) || EF.Functions.ILike(x.Technology.Summary, term));
        }

        if (Slugs.TryFrom(query.Field, out var fieldSlug))
        {
            source = source.Where(x => x.Field.Slug == fieldSlug);
        }

        var total = await source.CountAsync(cancellationToken).ConfigureAwait(false);

        var items = await source
            // Urutan tampil bidang dulu, baru nama: daftar yang dikelompokkan
            // menurut taksonomi lebih berguna daripada daftar abjad murni.
            .OrderBy(x => x.Field.DisplayOrder)
            .ThenBy(x => x.Technology.Name)
            .Skip((query.NormalizedPage - 1) * query.NormalizedPageSize)
            .Take(query.NormalizedPageSize)
            .Select(x => new TechnologySummaryResponse(
                x.Technology.Id,
                x.Technology.Slug,
                x.Technology.Name,
                x.Technology.Summary,
                x.Field.Slug,
                x.Field.Name,
                x.Technology.Maturity.ToString()))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PagedResponse<TechnologySummaryResponse>(items, query.NormalizedPage, query.NormalizedPageSize, total);
    }
}
