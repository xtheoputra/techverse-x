using Microsoft.EntityFrameworkCore;
using TechVerseX.Contracts.Technology;
using TechVerseX.TechnologyService.Domain;
using TechVerseX.TechnologyService.Infrastructure.Persistence;

namespace TechVerseX.TechnologyService.Features.EditContentSections;

/// <summary>
/// Kelima operasi yang mengisi bagian template ADR-012 pada satu topik.
/// </summary>
/// <remarks>
/// <b>Kenapa kelimanya satu irisan, bukan lima folder.</b> Pola vertical slice
/// memisahkan berdasarkan <em>alasan berubah</em>, dan kelima operasi ini berubah
/// karena alasan yang sama: template ADR-012. Ketiganya juga berbagi tiga langkah
/// yang identik — muat agregat berikut keempat koleksinya, panggil satu metode
/// agregat, kembalikan bentuk lengkap supaya <c>MissingSections</c> yang menyusut
/// langsung terlihat. Memecahnya jadi lima folder akan menyalin ketiga langkah itu
/// lima kali, dan salinan yang menyimpang adalah cacat yang sudah dua kali digigit
/// repo ini.
/// <para>
/// 🔑 <b>Tidak ada satu pun metode di sini yang menerima nomor langkah roadmap.</b>
/// Itu bukan kelalaian kontrak — <c>AddRoadmapStep</c> memberi nomor sendiri, dan
/// membuka jalan pintas ke sana akan mengembalikan roadmap berlubang jadi mungkin.
/// </para>
/// </remarks>
public sealed class EditContentSectionsHandler(TechnologyDbContext db)
{
    public Task<Outcome> SetPrerequisiteAsync(string slug, RoadmapStepRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return MutateAsync(
            slug,
            technology => technology.SetPrerequisite(request.Title, request.Description),
            cancellationToken);
    }

    public Task<Outcome> AddRoadmapStepAsync(string slug, RoadmapStepRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return MutateAsync(
            slug,
            technology => technology.AddRoadmapStep(request.Title, request.Description),
            cancellationToken);
    }

    public Task<Outcome> AddProjectAsync(string slug, ProjectRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return MutateAsync(
            slug,
            technology => technology.AddProject(request.Title, request.Brief),
            cancellationToken);
    }

    public Task<Outcome> AddResourceAsync(string slug, ResourceRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Jenis sumber enum, bukan teks bebas (ADR-012). Diurai DI SINI supaya
        // "Vidio" membalas 400 yang menyebutkan pilihan yang sah - bukan 500, dan
        // bukan pula diam-diam jatuh ke anggota pertama enum.
        if (!Enum.TryParse<ResourceType>(request.Type, ignoreCase: true, out var type)
            || !Enum.IsDefined(type))
        {
            return Task.FromResult(Outcome.Invalid(
                "type",
                $"Jenis sumber '{request.Type}' tidak dikenal. Yang sah: {string.Join(", ", Enum.GetNames<ResourceType>())}."));
        }

        return MutateAsync(
            slug,
            technology => technology.AddResource(type, request.Title, request.Url),
            cancellationToken);
    }

    public async Task<Outcome> AttachToolAsync(string slug, AttachToolRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!Slugs.TryFrom(request.ToolSlug, out var toolSlug))
        {
            return Outcome.Invalid("toolSlug", $"Slug alat '{request.ToolSlug}' bukan slug yang sah.");
        }

        var tool = await db.Tools
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Slug == toolSlug, cancellationToken)
            .ConfigureAwait(false);

        if (tool is null)
        {
            // 400, bukan 404: yang tidak ditemukan ada di dalam MUATAN, bukan di
            // alamat yang diminta - sama persis dengan alasan fieldSlug tak dikenal.
            return Outcome.Invalid(
                "toolSlug",
                $"Alat '{request.ToolSlug}' belum ada di katalog. Tambahkan lewat POST /api/v1/tools lebih dulu.");
        }

        return await MutateAsync(
            slug,
            technology => technology.AttachTool(tool.Id, request.Note),
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Menaikkan isi ke <c>draf</c>. Gagal kalau kelima bagian belum terisi, dan
    /// pesannya menyebut bagian mana.
    /// </summary>
    public Task<Outcome> MarkDraftedAsync(string slug, CancellationToken cancellationToken)
        => MutateAsync(slug, technology => technology.MarkDrafted(), cancellationToken);

    private async Task<Outcome> MutateAsync(string slug, Action<Technology> mutate, CancellationToken cancellationToken)
    {
        var technology = await db.Technologies
            .AsSplitQuery()
            .Include(t => t.Roadmap)
            .Include(t => t.Tools)
            .Include(t => t.Projects)
            .Include(t => t.Resources)
            .FirstOrDefaultAsync(t => t.Slug == slug, cancellationToken)
            .ConfigureAwait(false);

        if (technology is null)
        {
            return Outcome.NotFound();
        }

        try
        {
            mutate(technology);
        }
        catch (ArgumentException ex)
        {
            // Agregat menjaga bentuknya sendiri dan melempar untuk muatan yang
            // cacat - judul kosong, URL bukan http. Tugas lapisan ini mengubahnya
            // jadi 400, persis seperti yang ditulis komentar di Tool.Create.
            return Outcome.Invalid(ex.ParamName ?? "body", ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            // Bukan muatan yang cacat melainkan URUTAN yang salah - menambah
            // langkah roadmap sebelum ada prasyarat, atau menaikkan ke draf saat
            // bagiannya belum lengkap. Tetap 400: yang keliru permintaannya.
            return Outcome.Invalid("body", ex.Message);
        }

        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        technology.ClearEvents();

        var field = await db.Fields
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == technology.FieldId, cancellationToken)
            .ConfigureAwait(false);

        if (field is null)
        {
            return Outcome.NotFound();
        }

        var toolIds = technology.Tools.Select(link => link.ToolId).ToArray();

        var catalog = toolIds.Length == 0
            ? []
            : await db.Tools
                .AsNoTracking()
                .Where(tool => toolIds.Contains(tool.Id))
                .ToDictionaryAsync(tool => tool.Id, cancellationToken)
                .ConfigureAwait(false);

        return Outcome.Ok(TechnologyResponseFactory.From(technology, field, catalog));
    }

    /// <summary>
    /// Tiga akhir yang mungkin, dan tidak ada yang keempat: topiknya tidak ada
    /// (404), permintaannya keliru (400), atau berhasil dengan bentuk lengkap
    /// topik itu sesudah diubah.
    /// </summary>
    public sealed record Outcome(TechnologyResponse? Value, bool Missing, string? Field, string? Message)
    {
        public bool IsNotFound => Missing;

        public bool IsInvalid => Message is not null;

        public static Outcome Ok(TechnologyResponse value) => new(value, false, null, null);

        public static Outcome NotFound() => new(null, true, null, null);

        public static Outcome Invalid(string field, string message) => new(null, false, field, message);
    }
}
