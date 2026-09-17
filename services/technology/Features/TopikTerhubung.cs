using Microsoft.EntityFrameworkCore;
using TechVerseX.Contracts.Technology;
using TechVerseX.TechnologyService.Domain;
using TechVerseX.TechnologyService.Infrastructure.Persistence;

namespace TechVerseX.TechnologyService.Features;

/// <summary>
/// Relasi satu topik, siap dikirim: yang ia butuhkan dan yang membutuhkannya
/// (ADR-023).
/// </summary>
/// <remarks>
/// 🔑 <b>Satu pemuat, dipakai setiap jalan yang mengembalikan bentuk lengkap
/// topik</b> — <c>GetTechnologyHandler</c> dan <see cref="TopicMutation"/>. Kalau
/// jalan baca dan jalan tulis memuat relasi dengan caranya masing-masing, respons
/// sebuah PUT bagian isi bisa melaporkan relasi yang berbeda dari GET sesudahnya
/// untuk topik yang sama.
/// <para>
/// ⚠️ <b>Selalu satu lompatan.</b> Kedua kueri hanya melihat sisi yang menyentuh
/// topik ini langsung, jadi siklus yang lolos (SQL tangan, atau balapan yang
/// diterima di <c>RequireTopicHandler</c>) tidak pernah bisa membuat pembacaan
/// berputar atau meledak.
/// </para>
/// </remarks>
internal sealed record TopikTerhubung(
    IReadOnlyList<TechnologySummaryResponse> Requires,
    IReadOnlyList<TechnologySummaryResponse> RequiredBy)
{
    /// <summary>
    /// Tanpa relasi. <b>Hanya sah untuk topik yang baru dibuat di permintaan yang
    /// sama</b>: Id-nya belum bisa dirujuk sisi mana pun, sebab kedua ujung sisi
    /// berkunci asing.
    /// </summary>
    public static TopikTerhubung Kosong { get; } = new([], []);

    public static async Task<TopikTerhubung> MuatAsync(
        TechnologyDbContext db,
        Guid technologyId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(db);

        // Sisi KELUAR: baris milik agregat topik ini sendiri.
        var requires = await Ringkas(
                db,
                db.Technologies
                    .AsNoTracking()
                    .Where(t => t.Id == technologyId)
                    .SelectMany(t => t.Relationships)
                    .Where(r => r.Kind == RelationshipKind.Requires)
                    .Select(r => r.ToTechnologyId))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        // Sisi MASUK: baris milik agregat topik LAIN yang menunjuk ke sini. Tidak
        // pernah disimpan sebagai baris kedua; diturunkan di sini, saat dibaca.
        // Indeks ix_technology_relationships_to_technology_id ada untuk kueri ini.
        var requiredBy = await Ringkas(
                db,
                db.Technologies
                    .AsNoTracking()
                    .SelectMany(t => t.Relationships)
                    .Where(r => r.ToTechnologyId == technologyId && r.Kind == RelationshipKind.Requires)
                    .Select(r => r.FromTechnologyId))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new TopikTerhubung(requires, requiredBy);
    }

    /// <summary>
    /// Topik-topik ber-Id <paramref name="ids"/>, dalam bentuk ringkas.
    /// </summary>
    /// <remarks>
    /// ⚠️ <b>Proyeksi ini SALINAN dari <c>SearchTechnologyHandler</c></b> (Id, Slug,
    /// Name, Summary, bidang, <c>Maturity.ToString()</c>), dan urutannya sama dengan
    /// daftar tanpa kata kunci di sana: urutan tampil bidang, lalu nama. Kalau salah
    /// satunya berubah, ubah keduanya — yang menjaganya hanya komentar ini dan
    /// asersi kematangan serta nama bidang di <c>PrasyaratTopikTests</c>.
    /// <para>
    /// JOIN biasa, bukan LEFT JOIN, dengan sengaja: sejak <c>RelasiAntarTopik</c>
    /// kedua ujung sisi berkunci asing, jadi tidak ada sisi menggantung yang bisa
    /// dijatuhkan diam-diam oleh JOIN ini.
    /// </para>
    /// </remarks>
    private static IQueryable<TechnologySummaryResponse> Ringkas(TechnologyDbContext db, IQueryable<Guid> ids) =>
        ids
            .Join(db.Technologies.AsNoTracking(), id => id, t => t.Id, (id, t) => t)
            .Join(
                db.Fields.AsNoTracking(),
                t => t.FieldId,
                f => f.Id,
                (t, f) => new { Technology = t, Field = f })
            .OrderBy(x => x.Field.DisplayOrder)
            .ThenBy(x => x.Technology.Name)
            .Select(x => new TechnologySummaryResponse(
                x.Technology.Id,
                x.Technology.Slug,
                x.Technology.Name,
                x.Technology.Summary,
                x.Field.Slug,
                x.Field.Name,
                x.Technology.Maturity.ToString()));
}
