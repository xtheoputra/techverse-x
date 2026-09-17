using Microsoft.EntityFrameworkCore;
using TechVerseX.Contracts.Technology;
using TechVerseX.TechnologyService.Domain;
using TechVerseX.TechnologyService.Infrastructure.Persistence;

namespace TechVerseX.TechnologyService.Features.RequireTopic;

/// <summary>
/// Mencatat bahwa satu topik <b>membutuhkan</b> topik lain — sisi
/// <see cref="RelationshipKind.Requires"/> ADR-023.
/// </summary>
/// <remarks>
/// 🔑 <b>Irisan sendiri, bukan metode di <c>EditContentSectionsHandler</c>.</b>
/// Irisan itu satu karena seluruh operasinya berubah karena template ADR-012; relasi
/// antar-topik bukan bagian template dan berubah karena ADR-023. Yang dipakai
/// bersama hanya jalurnya, <see cref="TopicMutation"/>, bukan salinannya.
/// <para>
/// <b>Topik tujuan yang tidak ada membalas 400, bukan 404</b>, sama seperti alat
/// yang tidak ada di <c>AttachToolAsync</c>: yang keliru isi muatan, bukan alamat.
/// 404 disimpan untuk satu arti saja — topik di alamatnya tidak ada — dan itulah
/// yang membuat gerbang ADR-020 terbaca: sakelar hidup membalas 400 untuk muatan
/// uji, sakelar mati 404 karena rutenya tidak dipasang.
/// </para>
/// <para>
/// ⚠️ <b>Balapan yang diterima.</b> Pencarian tujuan, pembacaan sisi milik tujuan,
/// dan <c>SaveChanges</c> bukan satu transaksi. Dua penulis serentak bisa
/// menyelipkan pasangan saling mensyaratkan, dan tujuan yang dibuang di antaranya
/// jadi 500 dari kunci asingnya. Itu celah yang sama dengan pemeriksaan slug milik
/// bidang di <c>CreateTechnologyHandler</c>, dan diterima dengan alasan yang sama:
/// penulisnya tunggal (seed, atau alur ADR-021). Pembacaan tetap aman kalau siklus
/// lolos — <see cref="TopikTerhubung"/> selalu satu lompatan.
/// </para>
/// <para>
/// Pemanggil produksi: <b>belum ada</b>. Endpoint ini ikut hilang bersama permukaan
/// tulis lain (ADR-020); jalan produksinya kelak alur bergerbang ADR-021.
/// </para>
/// </remarks>
public sealed class RequireTopicHandler(TechnologyDbContext db)
{
    public async Task<TopicMutationOutcome> HandleAsync(string slug, RequireTopicRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!Slugs.TryFrom(request.TopicSlug, out var topicSlug))
        {
            return TopicMutationOutcome.Invalid("topicSlug", $"'{request.TopicSlug}' bukan slug topik yang sah.");
        }

        var topicId = await db.Technologies
            .AsNoTracking()
            .Where(t => t.Slug == topicSlug)
            .Select(t => (Guid?)t.Id)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (topicId is null)
        {
            // Dua sebab, satu kode status - jadi pesannya yang harus membedakan.
            // Bidang dan topik berbagi ruang nama /teknologi/<slug> (ADR-009), dan
            // "ai-agents" terlihat sah bagi penulis; yang perlu ia tahu adalah
            // perbaikan mana yang berlaku.
            var slugMilikBidang = await db.Fields
                .AnyAsync(f => f.Slug == topicSlug, cancellationToken)
                .ConfigureAwait(false);

            return slugMilikBidang
                ? TopicMutationOutcome.Invalid(
                    "topicSlug",
                    $"'{topicSlug}' adalah bidang, bukan topik. Relasi V1 hanya menghubungkan topik dengan topik (ADR-023).")
                : TopicMutationOutcome.Invalid(
                    "topicSlug",
                    $"Topik '{topicSlug}' tidak ada. Buat lewat POST /api/v1/technologies lebih dulu.");
        }

        // Sisi Requires milik TUJUAN, untuk aturan dua topik tidak boleh saling
        // mensyaratkan. Hari ini tujuan langsungnya saja; penutupan transitif
        // cukup diganti DI SINI, tanda tangan RequireTopic tidak berubah (ADR-023).
        var topicRequires = await db.Technologies
            .AsNoTracking()
            .Where(t => t.Id == topicId)
            .SelectMany(t => t.Relationships)
            .Where(r => r.Kind == RelationshipKind.Requires)
            .Select(r => r.ToTechnologyId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return await TopicMutation.RunAsync(
            db,
            slug,
            technology => technology.RequireTopic(topicId.Value, topicRequires),
            cancellationToken).ConfigureAwait(false);
    }
}
