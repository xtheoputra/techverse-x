using TechVerseX.Contracts.Technology;
using TechVerseX.TechnologyService.Domain;
using TechVerseX.TechnologyService.Infrastructure.Persistence;

namespace TechVerseX.TechnologyService.Features.EditMedia;

/// <summary>
/// Menetapkan dan membuang media sebuah topik (ADR-028 Tahap 3b).
/// </summary>
/// <remarks>
/// 🔑 <b>Irisan sendiri, bukan tambahan di <c>EditContentSections</c>:</b> irisan itu berubah
/// karena template ADR-012, sedangkan media berubah karena ADR-028 — dan media bukan bagian
/// kelima template (ia menghias bagian-bagian itu lewat <c>::media[kunci]</c> dan tak pernah
/// masuk <c>MissingSections</c>). Jalur muat → ubah → simpan → kembalikan tetap SATU:
/// <see cref="TopicMutation"/>.
/// <para>
/// Aturan bentuknya (teks alternatif wajib, lisensi wajib, sumber wajib kecuali karya sendiri,
/// gambar hanya berkas sendiri) tinggal di <see cref="TechnologyMedia"/>; di sini hanya
/// menerjemahkan jenis dari teks ke enum, <b>menurut nama saja</b>.
/// </para>
/// </remarks>
public sealed class EditMediaHandler(TechnologyDbContext db)
{
    public Task<TopicMutationOutcome> UpsertAsync(string slug, string key, MediaRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Jenis enum, bukan teks bebas — sama dengan jenis sumber (#60): BERDASARKAN NAMA SAJA,
        // karena Enum.TryParse menerima angka ("0") dan gabungan bendera ("Image, Video") yang
        // tersimpan sebagai anggota lain dan tak pernah dimaksudkan penulisnya.
        var nama = Enum.GetNames<MediaKind>()
            .FirstOrDefault(n => string.Equals(n, request.Kind, StringComparison.OrdinalIgnoreCase));

        if (nama is null)
        {
            return Task.FromResult(TopicMutationOutcome.Invalid(
                "kind",
                $"Jenis media '{request.Kind}' tidak dikenal. Yang sah: {string.Join(", ", Enum.GetNames<MediaKind>())}."));
        }

        var kind = Enum.Parse<MediaKind>(nama);

        return TopicMutation.RunAsync(
            db,
            slug,
            technology => technology.UpsertMedia(
                key, kind, request.Url, request.VideoId, request.Alt, request.Caption, request.SourceName, request.SourceUrl, request.License),
            cancellationToken);
    }

    /// <summary>Idempoten: membuang kunci yang tak ada adalah keadaan yang sudah tercapai, bukan 404.</summary>
    public Task<TopicMutationOutcome> RemoveAsync(string slug, string key, CancellationToken cancellationToken)
        => TopicMutation.RunAsync(db, slug, technology => technology.RemoveMedia(key), cancellationToken);
}
