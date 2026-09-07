using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using TechVerseX.Contracts.Technology;

namespace TechVerseX.TechnologyService.Features.CreateTechnology;

public static class CreateTechnologyEndpoint
{
    public static void MapCreateTechnology(this IEndpointRouteBuilder routes)
    {
        ArgumentNullException.ThrowIfNull(routes);

        routes.MapPost("/", HandleAsync)
            .WithName("CreateTechnology")
            .WithSummary("Membuat satu teknologi baru.")
            .ProducesValidationProblem();
    }

    private static async Task<Results<Created<TechnologyResponse>, ValidationProblem, Conflict<string>>> HandleAsync(
        CreateTechnologyRequest request,
        CreateTechnologyHandler handler,
        IValidator<CreateTechnologyCommand> validator,
        CancellationToken cancellationToken)
    {
        var command = new CreateTechnologyCommand(request.Name, request.Summary, request.Category, request.Slug);

        var validation = await validator.ValidateAsync(command, cancellationToken).ConfigureAwait(false);
        if (!validation.IsValid)
        {
            return TypedResults.ValidationProblem(validation.ToDictionary());
        }

        var result = await handler.HandleAsync(command, cancellationToken).ConfigureAwait(false);

        return result.IsConflict
            ? TypedResults.Conflict($"Slug '{result.ConflictingSlug}' sudah dipakai teknologi lain.")
            : TypedResults.Created($"/api/v1/technologies/{result.Value!.Slug}", result.Value);
    }
}
