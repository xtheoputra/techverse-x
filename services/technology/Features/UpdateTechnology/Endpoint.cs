using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using TechVerseX.Contracts.Technology;

namespace TechVerseX.TechnologyService.Features.UpdateTechnology;

public static class UpdateTechnologyEndpoint
{
    public static void MapUpdateTechnology(this IEndpointRouteBuilder routes)
    {
        ArgumentNullException.ThrowIfNull(routes);

        // PUT pada alamat topiknya sendiri — pasangan GET /{slug}. Idempoten: mengulang
        // muatan yang sama tidak mengubah apa pun, termasuk status tinjau.
        routes.MapPut("/{slug}", HandleAsync)
            .WithName("UpdateTechnology")
            .WithSummary("Mengganti nama dan ringkasan sebuah topik. Isi yang berbeda menggugurkan tinjau.")
            .ProducesValidationProblem();
    }

    private static async Task<Results<Ok<TechnologyResponse>, NotFound, ValidationProblem>> HandleAsync(
        string slug,
        UpdateTechnologyRequest request,
        UpdateTechnologyHandler handler,
        CancellationToken cancellationToken)
        => TopicMutation.ToHttpResult(await handler.HandleAsync(slug, request, cancellationToken).ConfigureAwait(false));
}
