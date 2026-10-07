using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using TechVerseX.Contracts.Technology;

namespace TechVerseX.TechnologyService.Features.EditMedia;

public static class EditMediaEndpoint
{
    public static void MapEditMedia(this IEndpointRouteBuilder routes)
    {
        ArgumentNullException.ThrowIfNull(routes);

        // PUT per kunci: kunci adalah ALAMAT media dari teks (::media[kunci]), jadi menetapkannya
        // lagi dengan isi yang sama tak mengubah apa pun (idempoten), dan isi yang berbeda
        // MENGGANTI — termasuk menggugurkan tinjau (ADR-012 Pembaruan 2026-10-07).
        routes.MapPut("/{slug}/media/{key}", UpsertAsync)
            .WithName("UpsertMedia")
            .WithSummary("Menetapkan satu media (gambar, diagram, atau video) topik ini menurut kuncinya. Idempoten.")
            .ProducesValidationProblem();

        routes.MapDelete("/{slug}/media/{key}", RemoveAsync)
            .WithName("RemoveMedia")
            .WithSummary("Membuang satu media menurut kuncinya. Idempoten.")
            .ProducesValidationProblem();
    }

    private static async Task<Results<Ok<TechnologyResponse>, NotFound, ValidationProblem>> UpsertAsync(
        string slug,
        string key,
        MediaRequest request,
        EditMediaHandler handler,
        CancellationToken cancellationToken)
        => TopicMutation.ToHttpResult(await handler.UpsertAsync(slug, key, request, cancellationToken).ConfigureAwait(false));

    private static async Task<Results<Ok<TechnologyResponse>, NotFound, ValidationProblem>> RemoveAsync(
        string slug,
        string key,
        EditMediaHandler handler,
        CancellationToken cancellationToken)
        => TopicMutation.ToHttpResult(await handler.RemoveAsync(slug, key, cancellationToken).ConfigureAwait(false));
}
