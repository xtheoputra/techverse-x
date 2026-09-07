using Microsoft.EntityFrameworkCore;
using TechVerseX.Contracts.Technology;
using TechVerseX.TechnologyService.Domain;
using TechVerseX.TechnologyService.Infrastructure.Persistence;

namespace TechVerseX.TechnologyService.Features.ListFields;

/// <summary>
/// Keempat belas bidang, berikut dua angka kemajuan per bidang.
/// </summary>
/// <remarks>
/// Tidak ada halaman, tidak ada penyaringan, tidak ada penomoran halaman —
/// daftarnya tertutup dan berjumlah <see cref="FieldCatalog.ExpectedCount"/>.
/// Menambahkan paging ke daftar yang tidak bisa bertambah tanpa migrasi hanya
/// menambah permukaan yang harus diuji.
/// </remarks>
public sealed class ListFieldsHandler(TechnologyDbContext db)
{
    public async Task<IReadOnlyList<FieldResponse>> HandleAsync(CancellationToken cancellationToken)
    {
        return await db.Fields
            .AsNoTracking()
            .OrderBy(f => f.DisplayOrder)
            .Select(f => new FieldResponse(
                f.Id,
                f.Slug,
                f.Name,
                f.Summary,
                f.Priority.ToString(),
                f.DisplayOrder,
                db.Technologies.Count(t => t.FieldId == f.Id),

                // Angka kedua inilah ukuran kemajuan proyek menurut
                // docs/RENCANA-V1.md. Yang pertama bisa dinaikkan mesin dalam
                // semenit; yang ini menuntut manusia memanggil MarkReviewed.
                db.Technologies.Count(t => t.FieldId == f.Id && t.Maturity == ContentMaturity.HumanReviewed)))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
