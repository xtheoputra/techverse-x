using Microsoft.EntityFrameworkCore;
using TechVerseX.Contracts.Technology;
using TechVerseX.TechnologyService.Domain;
using TechVerseX.TechnologyService.Infrastructure.Persistence;

namespace TechVerseX.TechnologyService.Features.CreateTechnology;

public sealed class CreateTechnologyHandler(TechnologyDbContext db)
{
    public async Task<Result> HandleAsync(CreateTechnologyCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var technology = Domain.Technology.Create(command.Name, command.Summary, command.Category, command.Slug);

        var slugTaken = await db.Technologies
            .AnyAsync(t => t.Slug == technology.Slug, cancellationToken)
            .ConfigureAwait(false);

        if (slugTaken)
        {
            return Result.SlugConflict(technology.Slug);
        }

        db.Technologies.Add(technology);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // Event sudah terkumpul di aggregate; penerbitannya menunggu bus (ADR-0005).
        technology.ClearEvents();

        return Result.Created(new TechnologyResponse(
            technology.Id,
            technology.Slug,
            technology.Name,
            technology.Summary,
            technology.Category,
            technology.Status.ToString(),
            technology.CreatedAt,
            technology.UpdatedAt));
    }

    public sealed record Result(TechnologyResponse? Value, string? ConflictingSlug)
    {
        public bool IsConflict => ConflictingSlug is not null;

        public static Result Created(TechnologyResponse value) => new(value, null);

        public static Result SlugConflict(string slug) => new(null, slug);
    }
}
