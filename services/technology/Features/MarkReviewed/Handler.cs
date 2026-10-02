using TechVerseX.Contracts.Technology;
using TechVerseX.TechnologyService.Infrastructure.Persistence;

namespace TechVerseX.TechnologyService.Features.MarkReviewed;

/// <summary>
/// Menaikkan satu topik ke <c>tinjau</c> — <see cref="Domain.ContentMaturity.HumanReviewed"/>,
/// lewat <c>Technology.MarkReviewed</c> (ADR-012, ADR-021).
/// </summary>
/// <remarks>
/// 🔑 <b>Irisan sendiri, dan ia memakai ulang <see cref="TopicMutation"/> — bukan
/// menyalinnya.</b> Persis seperti <c>RequireTopic</c>: langkah muat → ubah → simpan
/// → kembalikan bentuk lengkap sama dengan endpoint tulis lain, dan
/// <see cref="TopicMutation"/> yang menulis komentarnya sendiri sudah menyebut
/// <c>/tinjau</c> sebagai pemakai berikutnya.
/// <para>
/// Seluruh pekerjaan aturannya hidup di agregat, jadi handler ini tipis dengan
/// sengaja. Dua galat yang mungkin dari <c>MarkReviewed</c> keduanya jadi 400 lewat
/// jalur yang sama dengan endpoint tulis lain:
/// <list type="bullet">
///   <item><c>ArgumentException</c> (nama pemeriksa kosong) → 400 bermedan
///   <c>reviewer</c>, dari <c>ParamName</c>.</item>
///   <item><c>InvalidOperationException</c> (halaman masih <c>kurasi</c>, belum
///   <c>draf</c>) → 400 bermedan <c>body</c>.</item>
/// </list>
/// </para>
/// <para>
/// 🔴 <b>Nama pemeriksanya diterima apa adanya dari muatan — penjaganya di ALUR,
/// bukan di sini.</b> Di produksi endpoint ini hanya terjangkau lewat workflow
/// bergerbang ADR-021, yang mengisi <see cref="MarkReviewedRequest.Reviewer"/> dari
/// <c>github.actor</c> dan tidak punya masukan untuk menimpanya. Menambahkan
/// "pemeriksa seharusnya X" di sini justru mengembalikan nama teks-bebas yang
/// ADR-021 §2 buang.
/// </para>
/// <para>
/// Pemanggil produksi: <b>belum ada</b> sampai workflow ADR-021 dijalankan. Endpoint
/// ini ikut hilang bersama permukaan tulis lain (ADR-020).
/// </para>
/// </remarks>
public sealed class MarkReviewedHandler(TechnologyDbContext db)
{
    public async Task<TopicMutationOutcome> HandleAsync(string slug, MarkReviewedRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Satu panggilan metode agregat di dalam lambda, dan pencarian apa pun
        // diselesaikan SEBELUM RunAsync — aturan yang TopicMutation tuliskan setelah
        // sabotase RequireTopic membuktikan lambda yang melempar galat pemrograman
        // ikut jadi 400. Di sini tidak ada pencarian: MarkReviewed hanya butuh nama.
        return await TopicMutation.RunAsync(
            db,
            slug,
            technology => technology.MarkReviewed(request.Reviewer),
            cancellationToken).ConfigureAwait(false);
    }
}
