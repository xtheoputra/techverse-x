using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using TechVerseX.Contracts.Common;
using TechVerseX.Contracts.Technology;

namespace TechVerseX.TechnologyService.Features.SearchTechnology;

public static class SearchTechnologyEndpoint
{
    public static void MapSearchTechnology(this IEndpointRouteBuilder routes)
    {
        ArgumentNullException.ThrowIfNull(routes);

        routes.MapGet("/", HandleAsync)
            .WithName("SearchTechnology")
            .WithSummary("Mencari dan menyaring daftar teknologi. Saring per bidang dengan ?field=<slug>.");
    }

    private static async Task<Ok<PagedResponse<TechnologySummaryResponse>>> HandleAsync(
        SearchTechnologyHandler handler,
        CancellationToken cancellationToken,
        string? q = null,
        string? field = null,
        int page = 1,
        int pageSize = 20)
    {
        var result = await handler
            .HandleAsync(new SearchTechnologyQuery(q, field, page, pageSize), cancellationToken)
            .ConfigureAwait(false);

        return TypedResults.Ok(result);
    }
}
