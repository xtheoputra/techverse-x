using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using TechVerseX.Contracts.Technology;

namespace TechVerseX.TechnologyService.Features.Search;

public static class SearchEndpoint
{
    /// <summary>
    /// <c>GET /api/v1/search?q=…</c> — bidang dan topik sekaligus.
    /// </summary>
    /// <remarks>
    /// Grupnya sendiri, bukan sub-rute <c>/technologies</c>: jawabannya memuat
    /// dua jenis entitas, dan menaruhnya di bawah salah satunya akan menyiratkan
    /// kepemilikan yang tidak ada — alasan yang sama persis dengan
    /// <c>/api/v1/tools</c> di <c>TechnologyModule</c>.
    /// <para>
    /// Ini permukaan BACA. Ia dipasang di atas batas ADR-020 dan ikut tayang di
    /// produksi.
    /// </para>
    /// </remarks>
    public static void MapSearch(this IEndpointRouteBuilder routes)
    {
        ArgumentNullException.ThrowIfNull(routes);

        routes.MapGet("/", HandleAsync)
            .WithName("Search")
            .WithSummary("Mencari bidang dan topik sekaligus dengan satu kata kunci (?q=).");
    }

    private static async Task<Ok<SearchResponse>> HandleAsync(
        SearchHandler handler,
        CancellationToken cancellationToken,
        string? q = null,
        int page = 1,
        int pageSize = 20)
    {
        var result = await handler
            .HandleAsync(new SearchQuery(q, page, pageSize), cancellationToken)
            .ConfigureAwait(false);

        return TypedResults.Ok(result);
    }
}
