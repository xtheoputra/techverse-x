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

        var technology = await db.Technologies
            .AsNoTracking()
            // 🔑 AsSplitQuery, bukan satu JOIN besar. Empat koleksi anak dalam satu
            // kueri menghasilkan hasil kali kartesius: satu topik dengan 8 langkah
            // roadmap, 5 alat, 3 proyek dan 6 sumber mengembalikan 720 baris untuk
            // mengisi 22 objek. Split query menukarnya jadi lima kueri kecil.
            .AsSplitQuery()
            .Include(t => t.Roadmap)
            .Include(t => t.Tools)
            .Include(t => t.Projects)
            .Include(t => t.Resources)
            .FirstOrDefaultAsync(t => t.Slug == query.Slug, cancellationToken)
            .ConfigureAwait(false);

        if (technology is null)
        {
            return null;
        }

        var field = await db.Fields
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == technology.FieldId, cancellationToken)
            .ConfigureAwait(false);

        if (field is null)
        {
            // FieldId kunci asing yang tidak nullable, jadi ini seharusnya mustahil.
            // Ditulis eksplisit supaya kalau ia tetap terjadi, yang muncul 404 -
            // bukan NullReferenceException yang menyamar jadi 500.
            return null;
        }

        // Alat DITAUTKAN, tidak disalin (ADR-015): identitasnya hidup di katalog
        // `tools`, dan yang tersimpan di topik cuma ToolId + catatan. Karena itu
        // katalognya diambil terpisah, lalu dijodohkan saat memetakan.
        var toolIds = technology.Tools.Select(link => link.ToolId).ToArray();

        var catalog = toolIds.Length == 0
            ? []
            : await db.Tools
                .AsNoTracking()
                .Where(tool => toolIds.Contains(tool.Id))
                .ToDictionaryAsync(tool => tool.Id, cancellationToken)
                .ConfigureAwait(false);

        return TechnologyResponseFactory.From(technology, field, catalog);
    }
}
