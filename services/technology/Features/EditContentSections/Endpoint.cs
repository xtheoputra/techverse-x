using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using TechVerseX.Contracts.Technology;

namespace TechVerseX.TechnologyService.Features.EditContentSections;

/// <summary>
/// Pintu masuk kelima bagian template ADR-012 pada satu topik.
/// </summary>
/// <remarks>
/// Semuanya bersarang di bawah <c>/api/v1/technologies/{slug}</c> karena bagian
/// isi <b>tidak punya hidup di luar topiknya</b> — persis alasan
/// <c>TechnologyDbContext</c> tidak memberi mereka <c>DbSet</c> sendiri. Katalog
/// alat berbeda: ia dipakai bersama banyak topik, jadi ia punya rutenya sendiri.
/// </remarks>
public static class EditContentSectionsEndpoint
{
    public static void MapEditContentSections(this IEndpointRouteBuilder routes)
    {
        ArgumentNullException.ThrowIfNull(routes);

        // PUT, bukan POST: prasyarat adalah langkah 0 yang TUNGGAL, dan memanggil
        // ini dua kali mengganti isinya alih-alih menambah langkah kedua. Itu
        // idempoten, dan PUT yang mengatakannya.
        routes.MapPut("/{slug}/roadmap/prasyarat", SetPrerequisiteAsync)
            .WithName("SetPrerequisite")
            .WithSummary("Menetapkan prasyarat (langkah 0) roadmap sebuah topik.")
            .ProducesValidationProblem();

        routes.MapPost("/{slug}/roadmap", AddRoadmapStepAsync)
            .WithName("AddRoadmapStep")
            .WithSummary("Menambah satu langkah roadmap di ujung. Nomornya ditentukan server.")
            .ProducesValidationProblem();

        routes.MapPost("/{slug}/tools", AttachToolAsync)
            .WithName("AttachTool")
            .WithSummary("Menautkan satu alat katalog ke topik ini.")
            .ProducesValidationProblem();

        routes.MapPost("/{slug}/projects", AddProjectAsync)
            .WithName("AddProject")
            .WithSummary("Menambah satu Mini Project.")
            .ProducesValidationProblem();

        routes.MapPost("/{slug}/resources", AddResourceAsync)
            .WithName("AddResource")
            .WithSummary("Menambah satu sumber belajar.")
            .ProducesValidationProblem();

        routes.MapPost("/{slug}/draf", MarkDraftedAsync)
            .WithName("MarkDrafted")
            .WithSummary("Menaikkan isi ke tingkat draf. Menolak kalau ada bagian yang kosong.")
            .ProducesValidationProblem();
    }

    private static async Task<Results<Ok<TechnologyResponse>, NotFound, ValidationProblem>> SetPrerequisiteAsync(
        string slug,
        RoadmapStepRequest request,
        EditContentSectionsHandler handler,
        CancellationToken cancellationToken)
        => TopicMutation.ToHttpResult(await handler.SetPrerequisiteAsync(slug, request, cancellationToken).ConfigureAwait(false));

    private static async Task<Results<Ok<TechnologyResponse>, NotFound, ValidationProblem>> AddRoadmapStepAsync(
        string slug,
        RoadmapStepRequest request,
        EditContentSectionsHandler handler,
        CancellationToken cancellationToken)
        => TopicMutation.ToHttpResult(await handler.AddRoadmapStepAsync(slug, request, cancellationToken).ConfigureAwait(false));

    private static async Task<Results<Ok<TechnologyResponse>, NotFound, ValidationProblem>> AttachToolAsync(
        string slug,
        AttachToolRequest request,
        EditContentSectionsHandler handler,
        CancellationToken cancellationToken)
        => TopicMutation.ToHttpResult(await handler.AttachToolAsync(slug, request, cancellationToken).ConfigureAwait(false));

    private static async Task<Results<Ok<TechnologyResponse>, NotFound, ValidationProblem>> AddProjectAsync(
        string slug,
        ProjectRequest request,
        EditContentSectionsHandler handler,
        CancellationToken cancellationToken)
        => TopicMutation.ToHttpResult(await handler.AddProjectAsync(slug, request, cancellationToken).ConfigureAwait(false));

    private static async Task<Results<Ok<TechnologyResponse>, NotFound, ValidationProblem>> AddResourceAsync(
        string slug,
        ResourceRequest request,
        EditContentSectionsHandler handler,
        CancellationToken cancellationToken)
        => TopicMutation.ToHttpResult(await handler.AddResourceAsync(slug, request, cancellationToken).ConfigureAwait(false));

    private static async Task<Results<Ok<TechnologyResponse>, NotFound, ValidationProblem>> MarkDraftedAsync(
        string slug,
        EditContentSectionsHandler handler,
        CancellationToken cancellationToken)
        => TopicMutation.ToHttpResult(await handler.MarkDraftedAsync(slug, cancellationToken).ConfigureAwait(false));
}
