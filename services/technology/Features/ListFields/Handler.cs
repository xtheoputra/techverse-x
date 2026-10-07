using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;
using TechVerseX.Contracts.Technology;
using TechVerseX.TechnologyService.Domain;
using TechVerseX.TechnologyService.Infrastructure.Persistence;

namespace TechVerseX.TechnologyService.Features.ListFields;

/// <summary>
/// Seluruh bidang, berikut dua angka kemajuan per bidang.
/// </summary>
/// <remarks>
/// <b>Endpoint-nya tetap tanpa penyaringan dan tanpa penomoran halaman</b> —
/// daftarnya tertutup dan berjumlah <see cref="FieldCatalog.ExpectedCount"/>.
/// Menambahkan paging ke daftar yang tidak bisa bertambah tanpa migrasi hanya
/// menambah permukaan yang harus diuji.
/// <para>
/// 🔑 <b>Handler-nya yang kemudian tumbuh satu parameter, dan pemakainya cuma
/// satu: pencarian.</b> Ia ada di sini, bukan sebagai kueri kedua di
/// <c>SearchHandler</c>, karena proyeksi <see cref="FieldResponse"/> membawa dua
/// angka kemajuan yang dihitung dari subkueri. Menyalin proyeksi itu ke tempat
/// kedua adalah cara paling mudah membuat halaman muka dan halaman pencarian
/// memberi <em>dua angka berbeda untuk satu bidang yang sama</em>.
/// </para>
/// </remarks>
public sealed class ListFieldsHandler(TechnologyDbContext db)
{
    /// <param name="term">
    /// Kata kunci pencarian. <c>null</c> — yang dipakai endpoint
    /// <c>/api/v1/fields</c> — berarti seluruh bidang dikembalikan apa
    /// adanya.
    /// </param>
    /// <param name="cancellationToken">Pembatalan permintaan.</param>
    public async Task<IReadOnlyList<FieldResponse>> HandleAsync(
        string? term,
        CancellationToken cancellationToken)
    {
        var source = db.Fields.AsNoTracking();

        if (PencarianTeks.Bersihkan(term, out var bersih))
        {
            // Bentuknya sama persis dengan SearchTechnologyHandler — FTS untuk
            // pencocokan lintas urutan kata, pola AWAL KATA supaya kata yang baru
            // diketik separuh tetap ketemu. Lihat alasan lengkapnya di sana dan
            // ADR-022.
            var pola = PencarianTeks.PolaAwalKata(bersih);

            source = source.Where(f =>
                EF.Property<NpgsqlTsVector>(f, PencarianTeks.KolomVektor)
                    .Matches(EF.Functions.WebSearchToTsQuery(PencarianTeks.Konfigurasi, bersih))
                || Regex.IsMatch(f.Name, pola, RegexOptions.IgnoreCase)
                || Regex.IsMatch(f.Summary, pola, RegexOptions.IgnoreCase));
        }

        return await source
            // Urutan tampil ADR-010, bukan peringkat — bahkan saat menyaring.
            // Seluruh bidang muat di satu layar hasil, jadi tidak
            // ada yang terpotong oleh urutan; yang dibeli urutan tetap adalah
            // daftar yang letaknya tidak berpindah-pindah antar kata kunci.
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
