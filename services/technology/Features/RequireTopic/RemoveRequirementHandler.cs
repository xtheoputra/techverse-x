using Microsoft.EntityFrameworkCore;
using TechVerseX.TechnologyService.Domain;
using TechVerseX.TechnologyService.Infrastructure.Persistence;

namespace TechVerseX.TechnologyService.Features.RequireTopic;

/// <summary>
/// Membuang sisi <b>"membutuhkan"</b> sebuah topik — kebalikan
/// <see cref="RequireTopicHandler"/> (ADR-023).
/// </summary>
/// <remarks>
/// 🔑 <b>Idempoten, dan sengaja TIDAK sestrict POST.</b> <c>POST /requires</c>
/// MENEGASKAN adanya relasi, jadi tujuan yang bukan topik ditolak 400. <c>DELETE</c>
/// menegaskan <b>TIDAK adanya</b> — dan itu sudah benar walau tujuannya tidak pernah
/// jadi topik. Jadi slug sah yang bukan topik (termasuk slug bidang) membalas 200
/// tanpa mengubah apa pun; hanya slug yang bentuknya memang cacat yang ditolak 400.
/// <para>
/// 🔴 <b>Gerbang ADR-020 tetap terbaca:</b> dengan sakelar mati rutenya tidak dipasang
/// (404); dengan sakelar hidup handler ini dipanggil dan membalas 200 atau 400 atau
/// 404 topik-asal — tidak pernah 404 karena "rutenya tidak ada".
/// </para>
/// <para>
/// 🔑 <b>Irisan yang sama dengan <c>RequireTopic</c>, bukan salinannya.</b> Keduanya
/// menulis sisi ADR-023 dan memakai ulang <see cref="TopicMutation"/>; menaruhnya di
/// irisan terpisah akan menghidupkan lagi dua jalur yang bisa menyimpang.
/// </para>
/// <para>
/// Pemanggil produksi: <b>belum ada</b>; terjangkau lewat alur bergerbang ADR-021,
/// yang kini juga jalan perbaikan sisi yang keliru (ADR-021 Pembaruan 2026-09-17).
/// </para>
/// </remarks>
public sealed class RemoveRequirementHandler(TechnologyDbContext db)
{
    public async Task<TopicMutationOutcome> HandleAsync(string slug, string topicSlug, CancellationToken cancellationToken)
    {
        if (!Slugs.TryFrom(topicSlug, out var tujuanSlug))
        {
            return TopicMutationOutcome.Invalid("topicSlug", $"'{topicSlug}' bukan slug topik yang sah.");
        }

        // Pencarian selesai SEBELUM RunAsync — aturan TopicMutation. Tujuan null (slug
        // sah yang bukan topik) membuat buang jadi tanpa-operasi: tidak ada sisi ke
        // sana untuk dibuang, dan DELETE menegaskan ketiadaan, bukan keberadaan.
        var topicId = await db.Technologies
            .AsNoTracking()
            .Where(t => t.Slug == tujuanSlug)
            .Select(t => (Guid?)t.Id)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        return await TopicMutation.RunAsync(
            db,
            slug,
            technology =>
            {
                if (topicId is Guid id)
                {
                    technology.RemoveRequirement(id);
                }
            },
            cancellationToken).ConfigureAwait(false);
    }
}
