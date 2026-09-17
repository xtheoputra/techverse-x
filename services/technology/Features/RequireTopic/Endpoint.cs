using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using TechVerseX.Contracts.Technology;

namespace TechVerseX.TechnologyService.Features.RequireTopic;

/// <summary>
/// <c>POST /api/v1/technologies/{slug}/requires</c> — topik di alamat membutuhkan
/// topik di muatan (ADR-023).
/// </summary>
/// <remarks>
/// Satu rute per jenis relasi, bukan <c>/relationships</c> dengan medan jenis: rute
/// ini tidak mengurai teks jenis sama sekali, jadi celah <c>Enum.TryParse</c> yang
/// meluluskan angka (<c>"0"</c>) tidak punya jalan masuk di sini.
/// <para>
/// ⚠️ Bukan <c>/prasyarat</c>: kata itu sudah berarti langkah 0 roadmap
/// (<c>/roadmap/prasyarat</c>, ADR-012), yang tetap satu-satunya prasyarat prosa.
/// </para>
/// </remarks>
public static class RequireTopicEndpoint
{
    public static void MapRequireTopic(this IEndpointRouteBuilder routes)
    {
        ArgumentNullException.ThrowIfNull(routes);

        // POST, dan tetap idempoten: mengulang sisi yang sama membalas 200 tanpa
        // menambah baris. Bukan PUT /requires/{topicSlug} - tujuan di alamat
        // membuat "topik tujuan tidak ada" jadi 404 yang tidak bisa dibedakan dari
        // rute yang tidak dipasang (ADR-020).
        routes.MapPost("/{slug}/requires", RequireTopicAsync)
            .WithName("RequireTopic")
            .WithSummary("Mencatat bahwa topik ini membutuhkan topik lain lebih dulu. Idempoten.")
            .ProducesValidationProblem();
    }

    private static async Task<Results<Ok<TechnologyResponse>, NotFound, ValidationProblem>> RequireTopicAsync(
        string slug,
        RequireTopicRequest request,
        RequireTopicHandler handler,
        CancellationToken cancellationToken)
        => TopicMutation.ToHttpResult(await handler.HandleAsync(slug, request, cancellationToken).ConfigureAwait(false));
}
