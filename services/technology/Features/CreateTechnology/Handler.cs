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

        if (!Slugs.TryFrom(command.FieldSlug, out var fieldSlug))
        {
            return Result.UnknownField(command.FieldSlug);
        }

        var field = await db.Fields
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Slug == fieldSlug, cancellationToken)
            .ConfigureAwait(false);

        if (field is null)
        {
            return Result.UnknownField(command.FieldSlug);
        }

        var technology = Domain.Technology.Create(command.Name, command.Summary, field.Id, command.Slug);

        var slugTaken = await db.Technologies
            .AnyAsync(t => t.Slug == technology.Slug, cancellationToken)
            .ConfigureAwait(false);

        if (slugTaken)
        {
            return Result.SlugConflict(technology.Slug);
        }

        db.Technologies.Add(technology);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // Event sudah terkumpul di aggregate; penerbitannya menunggu bus (ADR-004).
        technology.ClearEvents();

        return Result.Created(new TechnologyResponse(
            technology.Id,
            technology.Slug,
            technology.Name,
            technology.Summary,
            field.Slug,
            field.Name,
            technology.Status.ToString(),
            technology.Maturity.ToString(),
            technology.ReviewedAt,
            technology.CreatedAt,
            technology.UpdatedAt));
    }

    public sealed record Result(TechnologyResponse? Value, string? ConflictingSlug, string? UnknownFieldSlug)
    {
        public bool IsConflict => ConflictingSlug is not null;

        public bool IsUnknownField => UnknownFieldSlug is not null;

        public static Result Created(TechnologyResponse value) => new(value, null, null);

        public static Result SlugConflict(string slug) => new(null, slug, null);

        /// <summary>
        /// Bidang tidak dikenal. Ini 400, bukan 404: yang salah muatan permintaan,
        /// bukan alamat yang diminta. Daftar bidang tertutup dan disemai migrasi
        /// (ADR-010), jadi slug di luar daftar itu memang muatan yang keliru.
        /// </summary>
        public static Result UnknownField(string slug) => new(null, null, slug);
    }
}
