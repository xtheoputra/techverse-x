using Microsoft.EntityFrameworkCore;
using TechVerseX.Contracts.Technology;
using TechVerseX.TechnologyService.Domain;
using TechVerseX.TechnologyService.Infrastructure.Persistence;

namespace TechVerseX.TechnologyService.Features.EditContentSections;

/// <summary>
/// Operasi yang mengisi bagian template ADR-012 pada satu topik, plus
/// menaikkannya ke <c>draf</c>.
/// </summary>
/// <remarks>
/// <b>Kenapa keenamnya satu irisan, bukan enam folder.</b> Pola vertical slice
/// memisahkan berdasarkan <em>alasan berubah</em>, dan keenam operasi ini —
/// prasyarat, langkah roadmap, alat, proyek, sumber, dan <c>MarkDrafted</c> —
/// berubah karena alasan yang sama: template ADR-012.
/// <para>
/// <b>Jalur yang mereka pakai bersama TIDAK tinggal di sini lagi.</b> Muat
/// agregat berikut koleksinya, panggil satu metode agregat, terjemahkan galatnya,
/// kembalikan bentuk lengkap supaya <c>MissingSections</c> yang menyusut langsung
/// terlihat — semua itu kini <see cref="TopicMutation"/>. Irisan yang berubah
/// karena alasan LAIN (<c>RequireTopic</c> untuk ADR-023, lalu <c>/tinjau</c>
/// ADR-021) memakai jalur yang sama alih-alih menyalinnya; salinan yang menyimpang
/// adalah cacat yang sudah dua kali digigit repo ini.
/// </para>
/// <para>
/// 🔑 <b>Nomor langkah roadmap tak pernah ISIAN, hanya ALAMAT.</b>
/// <c>AddRoadmapStep</c> memberi nomor sendiri, dan tak satu pun metode di sini menerima
/// nomor untuk membuat langkah — membuka jalan pintas ke sana akan mengembalikan roadmap
/// berlubang jadi mungkin. Satu-satunya metode yang menerima nomor,
/// <see cref="ReplaceRoadmapStepAsync"/>, hanya menunjuk langkah yang SUDAH ADA (sejak ADR-028
/// Tahap 2); nomor yang belum ada ditolak.
/// </para>
/// </remarks>
public sealed class EditContentSectionsHandler(TechnologyDbContext db)
{
    public Task<TopicMutationOutcome> SetPrerequisiteAsync(string slug, RoadmapStepRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return TopicMutation.RunAsync(
            db,
            slug,
            technology => technology.SetPrerequisite(request.Title, request.Description),
            cancellationToken);
    }

    public Task<TopicMutationOutcome> AddRoadmapStepAsync(string slug, RoadmapStepRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return TopicMutation.RunAsync(
            db,
            slug,
            technology => technology.AddRoadmapStep(request.Title, request.Description),
            cancellationToken);
    }

    /// <summary>
    /// Mengganti langkah roadmap yang sudah ada menurut nomornya (ADR-028 Tahap 2, #79).
    /// Nomor yang belum ada ditolak 400 — ia alamat, bukan isian.
    /// </summary>
    public Task<TopicMutationOutcome> ReplaceRoadmapStepAsync(string slug, int order, RoadmapStepRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return TopicMutation.RunAsync(
            db,
            slug,
            technology => technology.ReplaceRoadmapStep(order, request.Title, request.Description),
            cancellationToken);
    }

    public Task<TopicMutationOutcome> AddProjectAsync(string slug, ProjectRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return TopicMutation.RunAsync(
            db,
            slug,
            technology => technology.AddProject(request.Title, request.Brief),
            cancellationToken);
    }

    public Task<TopicMutationOutcome> AddResourceAsync(string slug, ResourceRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Jenis sumber enum, bukan teks bebas (ADR-012). Diurai DI SINI supaya
        // "Vidio" membalas 400 yang menyebutkan pilihan yang sah - bukan 500, dan
        // bukan pula diam-diam jatuh ke anggota pertama enum.
        //
        // 🔴 BERDASARKAN NAMA SAJA (#60), tanpa peka huruf besar-kecil. Enum.TryParse
        // tidak bisa dipakai di sini: ia juga menerima teks ANGKA ("0" tersimpan
        // sebagai OfficialDocs, " 1 " sebagai Video) dan gabungan BENDERA
        // ("Video, Paper" tersimpan sebagai Repository, 1 | 2). Enum.IsDefined
        // tidak menolak keduanya, sebab hasilnya memang anggota yang sah.
        var nama = Enum.GetNames<ResourceType>()
            .FirstOrDefault(n => string.Equals(n, request.Type, StringComparison.OrdinalIgnoreCase));

        if (nama is null)
        {
            return Task.FromResult(TopicMutationOutcome.Invalid(
                "type",
                $"Jenis sumber '{request.Type}' tidak dikenal. Yang sah: {string.Join(", ", Enum.GetNames<ResourceType>())}."));
        }

        var type = Enum.Parse<ResourceType>(nama);

        return TopicMutation.RunAsync(
            db,
            slug,
            technology => technology.AddResource(type, request.Title, request.Url),
            cancellationToken);
    }

    public async Task<TopicMutationOutcome> AttachToolAsync(string slug, AttachToolRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!Slugs.TryFrom(request.ToolSlug, out var toolSlug))
        {
            return TopicMutationOutcome.Invalid("toolSlug", $"Slug alat '{request.ToolSlug}' bukan slug yang sah.");
        }

        var tool = await db.Tools
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Slug == toolSlug, cancellationToken)
            .ConfigureAwait(false);

        if (tool is null)
        {
            // 400, bukan 404: yang tidak ditemukan ada di dalam MUATAN, bukan di
            // alamat yang diminta - sama persis dengan alasan fieldSlug tak dikenal.
            return TopicMutationOutcome.Invalid(
                "toolSlug",
                $"Alat '{request.ToolSlug}' belum ada di katalog. Tambahkan lewat POST /api/v1/tools lebih dulu.");
        }

        return await TopicMutation.RunAsync(
            db,
            slug,
            technology => technology.AttachTool(tool.Id, request.Note),
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Menaikkan isi ke <c>draf</c>. Gagal kalau kelima bagian belum terisi, dan
    /// pesannya menyebut bagian mana.
    /// </summary>
    public Task<TopicMutationOutcome> MarkDraftedAsync(string slug, CancellationToken cancellationToken)
        => TopicMutation.RunAsync(db, slug, technology => technology.MarkDrafted(), cancellationToken);

    // ---- Membuang bagian isi (ADR-012) ------------------------------------
    // Pasangan DELETE dari tambah di atas, dan semuanya IDEMPOTEN — seperti DELETE
    // sisi ADR-023: membuang yang tidak ada bukan kekeliruan melainkan keadaan yang
    // sudah tercapai. Langkah ROADMAP sengaja tidak punya pasangan buang: nomornya
    // berurut tanpa lubang, jadi membuang satu langkah menuntut penomoran ulang
    // (ADR-012 Pembaruan 2026-10-02).

    public Task<TopicMutationOutcome> RemoveResourceAsync(string slug, Guid resourceId, CancellationToken cancellationToken)
        => TopicMutation.RunAsync(db, slug, technology => technology.RemoveResource(resourceId), cancellationToken);

    public Task<TopicMutationOutcome> RemoveProjectAsync(string slug, Guid projectId, CancellationToken cancellationToken)
        => TopicMutation.RunAsync(db, slug, technology => technology.RemoveProject(projectId), cancellationToken);

    public async Task<TopicMutationOutcome> DetachToolAsync(string slug, string toolSlug, CancellationToken cancellationToken)
    {
        if (!Slugs.TryFrom(toolSlug, out var slugAlat))
        {
            return TopicMutationOutcome.Invalid("toolSlug", $"Slug alat '{toolSlug}' bukan slug yang sah.");
        }

        // Pencarian selesai SEBELUM RunAsync (aturan TopicMutation). Alat yang tak ada
        // di katalog tak mungkin tertaut, jadi melepasnya tanpa-operasi 200 — bukan 400
        // seperti AttachTool, yang MENEGASKAN adanya alat.
        var toolId = await db.Tools
            .AsNoTracking()
            .Where(t => t.Slug == slugAlat)
            .Select(t => (Guid?)t.Id)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        return await TopicMutation.RunAsync(
            db,
            slug,
            technology =>
            {
                if (toolId is Guid id)
                {
                    technology.DetachTool(id);
                }
            },
            cancellationToken).ConfigureAwait(false);
    }
}
