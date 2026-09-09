using Microsoft.EntityFrameworkCore;
using TechVerseX.Contracts.Technology;
using TechVerseX.TechnologyService.Domain;
using TechVerseX.TechnologyService.Infrastructure.Persistence;

namespace TechVerseX.TechnologyService.Features.CreateTool;

public sealed record CreateToolCommand(string Name, string Summary, string? Homepage, string? Slug);

/// <summary>
/// Menambah satu alat ke <b>katalog</b>.
/// </summary>
/// <remarks>
/// 🔑 <b>Katalog berdiri sendiri karena alat DITAUTKAN, bukan disalin</b> — itu
/// bentuk yang sudah ditegakkan tipe (<c>Technology.Tools</c> bertipe
/// <c>IReadOnlyList&lt;TechnologyTool&gt;</c>, bukan <c>Tool</c>). Kalau menautkan
/// alat boleh membuat alatnya sekaligus, dua topik yang menyebut "Docker" akan
/// melahirkan dua baris katalog, dan "alat yang sama" berhenti punya arti.
/// Karena itu menautkan menuntut alatnya <b>sudah ada</b>, dan inilah pintunya.
/// </remarks>
public sealed class CreateToolHandler(TechnologyDbContext db)
{
    public async Task<Result> HandleAsync(CreateToolCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        Tool tool;

        try
        {
            tool = Tool.Create(command.Name, command.Summary, command.Homepage, command.Slug);
        }
        catch (ArgumentException ex)
        {
            // Slugs.From MELEMPAR untuk masukan yang tidak menyisakan karakter apa
            // pun ("!!!"). Tool.Create sendiri menulis bahwa mengubahnya jadi 400
            // adalah tugas lapisan ini.
            return Result.Invalid(ex.ParamName ?? "name", ex.Message);
        }

        var taken = await db.Tools
            .AnyAsync(t => t.Slug == tool.Slug, cancellationToken)
            .ConfigureAwait(false);

        if (taken)
        {
            return Result.Conflict(tool.Slug);
        }

        db.Tools.Add(tool);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Created(new ToolResponse(tool.Id, tool.Slug, tool.Name, tool.Summary, tool.Homepage));
    }

    public sealed record Result(ToolResponse? Value, string? ConflictingSlug, string? Field, string? Message)
    {
        public bool IsConflict => ConflictingSlug is not null;

        public bool IsInvalid => Message is not null;

        public static Result Created(ToolResponse value) => new(value, null, null, null);

        public static Result Conflict(string slug) => new(null, slug, null, null);

        public static Result Invalid(string field, string message) => new(null, null, field, message);
    }
}
