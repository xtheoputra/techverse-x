using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using TechVerseX.Contracts.Technology;
using TechVerseX.TechnologyService.Domain;
using TechVerseX.TechnologyService.Infrastructure.Persistence;

namespace TechVerseX.TechnologyService.Features;

/// <summary>
/// Tiga akhir yang mungkin, dan tidak ada yang keempat: topiknya tidak ada
/// (404), permintaannya keliru (400), atau berhasil dengan bentuk lengkap
/// topik itu sesudah diubah.
/// </summary>
public sealed record TopicMutationOutcome(TechnologyResponse? Value, bool Missing, string? Field, string? Message)
{
    public bool IsNotFound => Missing;

    public bool IsInvalid => Message is not null;

    public static TopicMutationOutcome Ok(TechnologyResponse value) => new(value, false, null, null);

    public static TopicMutationOutcome NotFound() => new(null, true, null, null);

    public static TopicMutationOutcome Invalid(string field, string message) => new(null, false, field, message);
}

/// <summary>
/// Jalur <b>muat → ubah → simpan → kembalikan bentuk lengkap</b> untuk setiap
/// endpoint tulis yang mengubah satu topik lewat satu metode agregatnya.
/// </summary>
/// <remarks>
/// 🔑 <b>Kenapa berkas ini ada di luar irisan mana pun.</b> Jalur ini dulu metode
/// privat <c>EditContentSectionsHandler</c>, dan irisan itu satu karena seluruh
/// operasinya berubah karena alasan yang SAMA: template ADR-012. Relasi antar-topik
/// berubah karena ADR-023, jadi ia irisannya sendiri (<c>RequireTopic</c>) — tapi
/// langkah muat, terjemahan galat, dan pemetaan responsnya persis sama. Menyalinnya
/// ke irisan kedua akan menghidupkan lagi salinan yang menyimpang, cacat yang sudah
/// dua kali digigit repo ini; menaruh <c>RequireTopic</c> di irisan template akan
/// membuat alasan irisan itu bohong. Jalur bersamanya pindah ke sini, dan
/// <c>/tinjau</c> ADR-021 nanti memakainya juga.
/// </remarks>
internal static class TopicMutation
{
    public static async Task<TopicMutationOutcome> RunAsync(
        TechnologyDbContext db,
        string slug,
        Action<Technology> mutate,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(mutate);

        var technology = await db.Technologies
            .AsSplitQuery()
            .Include(t => t.Roadmap)
            .Include(t => t.Tools)
            .Include(t => t.Projects)
            .Include(t => t.Resources)
            // Media (ADR-028 Tahap 3b): UpsertMedia memutuskan "kunci ini sudah ada, ganti"
            // dari koleksi yang dimuat DI SINI. Tanpa Include ia selalu kosong, kunci yang
            // diulang menambah baris kedua, dan indeks unik (TechnologyId, Key) menolaknya jadi
            // 500 — idempotensi PUT hilang tanpa satu baris pun di irisan media berubah.
            .Include(t => t.Media)
            // 🔴 WAJIB, walau tidak satu pun bagian template memakainya.
            // Technology.RequireTopic memutuskan "sisi ini sudah ada, jangan tambah"
            // dari koleksi yang dimuat DI SINI. Tanpa Include ia selalu kosong,
            // permintaan yang diulang menambah baris kedua, dan indeks unik
            // ix_technology_relationships_edge menolaknya jadi 500 — idempotensi
            // endpoint /requires hilang tanpa satu baris pun di irisannya berubah.
            .Include(t => t.Relationships)
            .FirstOrDefaultAsync(t => t.Slug == slug, cancellationToken)
            .ConfigureAwait(false);

        if (technology is null)
        {
            return TopicMutationOutcome.NotFound();
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
            return TopicMutationOutcome.Invalid(ex.ParamName ?? "body", ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            // Bukan muatan yang cacat melainkan URUTAN yang salah - menambah
            // langkah roadmap sebelum ada prasyarat, atau menaikkan ke draf saat
            // bagiannya belum lengkap. Tetap 400: yang keliru permintaannya.
            //
            // ⚠️ Tangkapan ini tidak bisa membedakan galat AGREGAT dari galat
            // pemrograman di dalam lambda. Diukur saat RequireTopicHandler
            // disabotase (cabang tujuan-tak-ada dibuang, topicId!.Value dipanggil
            // di dalam lambda): jawabannya bukan 500, melainkan 400
            // {"body":["Nullable object must have a value."]}. Karena itu setiap
            // pencarian diselesaikan SEBELUM RunAsync, dan lambda cukup satu
            // panggilan metode agregat.
            return TopicMutationOutcome.Invalid("body", ex.Message);
        }

        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        technology.ClearEvents();

        var field = await db.Fields
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == technology.FieldId, cancellationToken)
            .ConfigureAwait(false);

        if (field is null)
        {
            return TopicMutationOutcome.NotFound();
        }

        var toolIds = technology.Tools.Select(link => link.ToolId).ToArray();

        var catalog = toolIds.Length == 0
            ? []
            : await db.Tools
                .AsNoTracking()
                .Where(tool => toolIds.Contains(tool.Id))
                .ToDictionaryAsync(tool => tool.Id, cancellationToken)
                .ConfigureAwait(false);

        // Dimuat SESUDAH SaveChanges, lewat pemuat yang sama dengan GET: respons
        // tulis apa pun — termasuk PUT prasyarat pada topik yang punya sisi —
        // melaporkan relasi yang sama dengan yang akan dibaca pengunjung.
        var related = await TopikTerhubung.MuatAsync(db, technology.Id, cancellationToken).ConfigureAwait(false);

        return TopicMutationOutcome.Ok(TechnologyResponseFactory.From(technology, field, related, catalog));
    }

    /// <summary>
    /// Satu penerjemah untuk seluruh endpoint tulis per-topik. Menaruhnya di satu
    /// tempat memastikan muatan yang keliru menjadi <b>400, bukan 500</b> secara
    /// seragam — pelajaran issue #26 yang mahal justru karena hanya berlaku di
    /// sebagian jalur.
    /// </summary>
    public static Results<Ok<TechnologyResponse>, NotFound, ValidationProblem> ToHttpResult(
        TopicMutationOutcome outcome)
    {
        ArgumentNullException.ThrowIfNull(outcome);

        if (outcome.IsNotFound)
        {
            return TypedResults.NotFound();
        }

        if (outcome.IsInvalid)
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                [outcome.Field ?? "body"] = [outcome.Message!],
            });
        }

        return TypedResults.Ok(outcome.Value!);
    }
}
