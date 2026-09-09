using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using TechVerseX.Contracts.Technology;

namespace TechVerseX.TechnologyService.Features.CreateTool;

public static class CreateToolEndpoint
{
    public static void MapCreateTool(this IEndpointRouteBuilder routes)
    {
        ArgumentNullException.ThrowIfNull(routes);

        routes.MapPost("/", HandleAsync)
            .WithName("CreateTool")
            .WithSummary("Menambah satu alat ke katalog.")
            .ProducesValidationProblem();
    }

    private static async Task<Results<Created<ToolResponse>, ValidationProblem, Conflict<string>>> HandleAsync(
        CreateToolRequest request,
        CreateToolHandler handler,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await handler
            .HandleAsync(new CreateToolCommand(request.Name, request.Summary, request.Homepage, request.Slug), cancellationToken)
            .ConfigureAwait(false);

        if (result.IsInvalid)
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                [result.Field ?? "name"] = [result.Message!],
            });
        }

        return result.IsConflict
            ? TypedResults.Conflict($"Slug alat '{result.ConflictingSlug}' sudah dipakai.")
            : TypedResults.Created($"/api/v1/tools/{result.Value!.Slug}", result.Value);
    }
}
