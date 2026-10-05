using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using TechVerseX.Contracts.Technology;

namespace TechVerseX.TechnologyService.Features.ListFields;

public static class ListFieldsEndpoint
{
    public static void MapListFields(this IEndpointRouteBuilder routes)
    {
        ArgumentNullException.ThrowIfNull(routes);

        routes.MapGet("/", HandleAsync)
            .WithName("ListFields")
            .WithSummary("Empat belas bidang teknologi, berikut jumlah topik dan berapa yang sudah diperiksa manusia.");
    }

    private static async Task<Ok<IReadOnlyList<FieldResponse>>> HandleAsync(
        ListFieldsHandler handler,
        CancellationToken cancellationToken)
    {
        // term: null — endpoint ini memang mengembalikan keempat belas bidang apa
        // adanya. Penyaringan hanya dipakai pencarian; lihat catatan di handler.
        var result = await handler.HandleAsync(term: null, cancellationToken).ConfigureAwait(false);

        return TypedResults.Ok(result);
    }
}
