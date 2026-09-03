using Microsoft.EntityFrameworkCore;
using TechVerseX.Contracts.Technology;
using TechVerseX.TechnologyService.Infrastructure.Persistence;

namespace TechVerseX.TechnologyService.Features.GetTechnology;

public sealed record GetTechnologyQuery(string Slug);

public sealed class GetTechnologyHandler(TechnologyDbContext db)
{
    public async Task<TechnologyResponse?> HandleAsync(GetTechnologyQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        return await db.Technologies
            .AsNoTracking()
            .Where(t => t.Slug == query.Slug)
            .Select(t => new TechnologyResponse(
                t.Id,
                t.Slug,
                t.Name,
                t.Summary,
                t.Category,
                t.Status.ToString(),
                t.CreatedAt,
                t.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
