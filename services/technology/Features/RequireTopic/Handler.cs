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
/// ⚠️ <b>Balapan yang diterima.</b> Pencarian tujuan, pembacaan penutupan milik
/// tujuan, dan <c>SaveChanges</c> bukan satu transaksi. Dua penulis serentak bisa
/// menyelipkan siklus, dan tujuan yang dibuang di antaranya jadi 500 dari kunci
/// asingnya. Itu celah yang sama dengan pemeriksaan slug milik bidang di
/// <c>CreateTechnologyHandler</c>, dan diterima dengan alasan yang sama: penulisnya
/// tunggal (seed, atau alur ADR-021). Pembacaan tetap aman kalau siklus lolos —
/// <see cref="TopikTerhubung"/> selalu satu lompatan. Yang TIDAK aman lagi begitu
/// ada fitur yang mengurutkan topik lewat sisi <c>Requires</c>, dan itulah pemicu
/// yang membuat pemeriksaan ini naik dari satu lompatan ke penutupan transitif.
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

        // PENUTUPAN TRANSITIF sisi Requires milik TUJUAN — bukan tujuan langsungnya.
        // Sampai 2026-09-24 yang diserahkan cuma satu lompatan, dan A→B→C→A lolos:
        // menambah A→B diterima karena B tidak langsung mensyaratkan A, walau C
        // mensyaratkannya. Tanda tangan RequireTopic memang tidak berubah, seperti
        // yang diramal ADR-023 - tapi PESAN galatnya berubah, sebab "dua topik tidak
        // boleh saling mensyaratkan" adalah kalimat yang SALAH untuk siklus bertiga.
        var topicRequires = await PenutupanRequiresAsync(topicId.Value, cancellationToken).ConfigureAwait(false);

        return await TopicMutation.RunAsync(
            db,
            slug,
            technology => technology.RequireTopic(topicId.Value, topicRequires),
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Semua topik yang <paramref name="awal"/> butuhkan, langsung maupun lewat
    /// rantai — penutupan transitif sisi <see cref="RelationshipKind.Requires"/>.
    /// </summary>
    /// <remarks>
    /// 🔑 <b>Penyapuan berlapis, bukan CTE rekursif.</b> Yang pertama tetap LINQ EF
    /// biasa dan tidak menambahkan SQL mentah pertama ke lapisan fitur; yang kedua
    /// lebih hemat perjalanan tapi menuntut nama tabel dan skema ditulis tangan,
    /// terlepas dari pemetaan EF yang menjaganya. Harganya jujur: <b>kedalaman + 1</b>
    /// perjalanan ke basis data. Untuk V1 itu kecil — target Bulan 6 dua puluh dua
    /// topik (RENCANA-V1), dan rantai prasyarat yang lebih dalam dari beberapa
    /// lompatan bukan roadmap yang bisa dibaca manusia.
    /// <para>
    /// ⚠️ <b>Berhenti walau barisnya SUDAH berputar.</b> Aturan ini lahir sesudah
    /// tabelnya ada, jadi siklus yang lolos sebelum hari ini mungkin sudah tersimpan;
    /// <c>tercapai</c> yang menyaring lapisan berikutnya membuat tiap topik dikunjungi
    /// sekali. Tanpa penyaring itu, satu siklus lama akan menggantung permintaannya.
    /// </para>
    /// <para>
    /// Titik awalnya tidak <b>disemai</b> ke hasil — yang dikembalikan apa yang ia
    /// butuhkan, bukan dirinya — tapi ia <b>bisa</b> muncul di sana kalau baris yang
    /// sudah tersimpan berputar melewatinya. Itu tidak pernah menolak sisi yang sah:
    /// yang dicari <c>RequireTopic</c> adalah Id topik ASAL, dan asal tidak mungkin
    /// sama dengan tujuan — pemeriksaan sisi-ke-diri-sendiri sudah melewatinya lebih
    /// dulu. Topik tanpa sisi menjawab himpunan kosong, persis seperti kueri satu
    /// lompatan yang digantinya.
    /// </para>
    /// </remarks>
    private async Task<List<Guid>> PenutupanRequiresAsync(Guid awal, CancellationToken cancellationToken)
    {
        var tercapai = new HashSet<Guid>();
        var batas = new List<Guid> { awal };

        while (batas.Count > 0)
        {
            var lapisan = await db.Technologies
                .AsNoTracking()
                .Where(t => batas.Contains(t.Id))
                .SelectMany(t => t.Relationships)
                .Where(r => r.Kind == RelationshipKind.Requires)
                .Select(r => r.ToTechnologyId)
                .Distinct()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            var baru = new List<Guid>();
            foreach (var id in lapisan)
            {
                if (tercapai.Add(id))
                {
                    baru.Add(id);
                }
            }

            batas = baru;
        }

        return [.. tercapai];
    }
}
