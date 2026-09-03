using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using TechVerseX.Contracts.Technology;

namespace TechVerseX.TechnologyService.Features.GetTechnology;

public static class GetTechnologyEndpoint
{
    public static void MapGetTechnology(this IEndpointRouteBuilder routes)
    {
        ArgumentNullException.ThrowIfNull(routes);

        routes.MapGet("/{slug}", HandleAsync)
            .WithName("GetTechnology")
            .WithSummary("Mengambil satu teknologi berdasarkan slug.");
    }

    private static async Task<Results<Ok<TechnologyResponse>, NotFound>> HandleAsync(
        string slug,
        GetTechnologyHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new GetTechnologyQuery(slug), cancellationToken).ConfigureAwait(false);

        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }
}
