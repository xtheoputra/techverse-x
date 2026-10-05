using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using TechVerseX.Contracts.Technology;

namespace TechVerseX.TechnologyService.Features.MarkReviewed;

/// <summary>
/// <c>POST /api/v1/technologies/{slug}/tinjau</c> — topik di alamat sudah
/// diperiksa manusia (ADR-012, ADR-021).
/// </summary>
/// <remarks>
/// Duduk di bawah cabang <c>editorialWrites</c> bersama endpoint bagian isi dan
/// <c>/requires</c> (ADR-020): ia ikut hilang di produksi tanpa aturan tambahan,
/// dan <c>PermukaanTulisTests</c> menuntutnya hilang begitu ia masuk daftar
/// <c>SemuaEndpointTulis</c>. Jalan produksinya satu-satunya: alur bergerbang
/// ADR-021 yang memanggil rute yang sama lewat <c>localhost</c> di dalam runner.
/// <para>
/// 🔑 <b>Alamatnya <c>/tinjau</c>, bukan <c>/review</c>:</b> ia menamai KEADAAN yang
/// dituju (<c>tinjau</c> = <see cref="Domain.ContentMaturity.HumanReviewed"/>),
/// idiom yang sama dengan <c>/draf</c>.
/// </para>
/// </remarks>
public static class MarkReviewedEndpoint
{
    public static void MapMarkReviewed(this IEndpointRouteBuilder routes)
    {
        ArgumentNullException.ThrowIfNull(routes);

        // POST, dan idempoten untuk keadaan: menaikkan halaman yang sudah tinjau
        // dengan nama pemeriksa yang sama tetap 200. Bukan PUT /maturity: tinjau
        // satu-satunya naik yang menuntut nama manusia (MarkReviewed), dan
        // menyatukannya dengan draf/kurasi di satu rute akan menyembunyikan itu.
        routes.MapPost("/{slug}/tinjau", MarkReviewedAsync)
            .WithName("MarkReviewed")
            .WithSummary("Menaikkan topik ke tinjau — kelima bagiannya sudah diperiksa manusia. Menuntut nama pemeriksa.")
            .ProducesValidationProblem();
    }

    private static async Task<Results<Ok<TechnologyResponse>, NotFound, ValidationProblem>> MarkReviewedAsync(
        string slug,
        MarkReviewedRequest request,
        MarkReviewedHandler handler,
        CancellationToken cancellationToken)
        => TopicMutation.ToHttpResult(await handler.HandleAsync(slug, request, cancellationToken).ConfigureAwait(false));
}
