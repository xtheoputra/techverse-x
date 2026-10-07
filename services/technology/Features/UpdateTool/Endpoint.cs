using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using TechVerseX.Contracts.Technology;

namespace TechVerseX.TechnologyService.Features.UpdateTool;

public static class UpdateToolEndpoint
{
    public static void MapUpdateTool(this IEndpointRouteBuilder routes)
    {
        ArgumentNullException.ThrowIfNull(routes);

        // PUT pada alamat alatnya — slug di alamat, tak pernah di badan (tak bisa diubah).
        routes.MapPut("/{slug}", HandleAsync)
            .WithName("UpdateTool")
            .WithSummary("Mengganti nama, ringkasan, dan beranda satu alat katalog. Teks yang berbeda menggugurkan tinjau topik yang menautkannya.")
            .ProducesValidationProblem();
    }

    private static async Task<Results<Ok<ToolResponse>, NotFound, ValidationProblem>> HandleAsync(
        string slug,
        UpdateToolRequest request,
        UpdateToolHandler handler,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await handler
            .HandleAsync(new UpdateToolCommand(slug, request.Name, request.Summary, request.Homepage), cancellationToken)
            .ConfigureAwait(false);

        if (result.IsNotFound)
        {
            return TypedResults.NotFound();
        }

        if (result.IsInvalid)
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                [result.Field ?? "name"] = [result.Message!],
            });
        }

        return TypedResults.Ok(result.Value!);
    }
}
