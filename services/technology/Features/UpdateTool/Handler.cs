using Microsoft.EntityFrameworkCore;
using TechVerseX.Contracts.Technology;
using TechVerseX.TechnologyService.Domain;
using TechVerseX.TechnologyService.Infrastructure.Persistence;

namespace TechVerseX.TechnologyService.Features.UpdateTool;

public sealed record UpdateToolCommand(string Slug, string Name, string Summary, string? Homepage);

/// <summary>
/// Mengganti satu alat di <b>katalog</b> (ADR-028 Tahap 2, #79).
/// </summary>
/// <remarks>
/// 🔑 <b>Alat dipakai bersama, jadi mengubahnya bisa mengubah halaman yang sudah
/// diperiksa.</b> Nama dan ringkasan alat tampil di setiap topik yang menautkannya
/// (ADR-015). Karena itu, bila TEKS berubah, setiap topik <c>tinjau</c> yang menautkan
/// alat ini ikut menggugurkan pemeriksaannya — aturan yang sama dengan mengganti catatan
/// alat atau prasyarat (keputusan pemilik 2026-10-06). Semuanya satu <c>SaveChanges</c>,
/// jadi katalog dan topik-topiknya tak pernah terlihat setengah berubah.
/// <para>
/// Hanya topik yang <b>sudah diperiksa</b> yang dimuat: topik lain tak punya
/// pemeriksaan untuk digugurkan, dan menyentuhnya hanya menggeser <c>UpdatedAt</c>
/// tanpa arti.
/// </para>
/// </remarks>
public sealed class UpdateToolHandler(TechnologyDbContext db)
{
    public async Task<Result> HandleAsync(UpdateToolCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var tool = await db.Tools
            .FirstOrDefaultAsync(t => t.Slug == command.Slug, cancellationToken)
            .ConfigureAwait(false);

        if (tool is null)
        {
            return Result.NotFound();
        }

        bool textChanged;

        try
        {
            textChanged = tool.Update(command.Name, command.Summary, command.Homepage);
        }
        catch (ArgumentException ex)
        {
            return Result.Invalid(ex.ParamName ?? "name", ex.Message);
        }

        var digugurkan = new List<Technology>();

        if (textChanged)
        {
            // Include(Tools) WAJIB: LinkedToolTextChanged memutuskan "alat ini milik
            // topik ini" dari koleksi yang dimuat DI SINI — tanpanya ia selalu kosong
            // dan tak ada pemeriksaan yang gugur, tanpa satu galat pun.
            digugurkan = await db.Technologies
                .Include(t => t.Tools)
                .Where(t => t.Maturity == ContentMaturity.HumanReviewed
                    && t.Tools.Any(link => link.ToolId == tool.Id))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            foreach (var technology in digugurkan)
            {
                technology.LinkedToolTextChanged(tool.Id);
            }
        }

        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        foreach (var technology in digugurkan)
        {
            technology.ClearEvents();
        }

        return Result.Updated(new ToolResponse(tool.Id, tool.Slug, tool.Name, tool.Summary, tool.Homepage));
    }

    public sealed record Result(ToolResponse? Value, bool Missing, string? Field, string? Message)
    {
        public bool IsNotFound => Missing;

        public bool IsInvalid => Message is not null;

        public static Result Updated(ToolResponse value) => new(value, false, null, null);

        public static Result NotFound() => new(null, true, null, null);

        public static Result Invalid(string field, string message) => new(null, false, field, message);
    }
}
