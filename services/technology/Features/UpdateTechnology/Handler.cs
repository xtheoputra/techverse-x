using TechVerseX.Contracts.Technology;
using TechVerseX.TechnologyService.Infrastructure.Persistence;

namespace TechVerseX.TechnologyService.Features.UpdateTechnology;

/// <summary>
/// Mengganti nama dan ringkasan satu topik (ADR-028 Tahap 2, #79).
/// </summary>
/// <remarks>
/// 🔑 <b>Tidak ada logika baru di sini.</b> Penurunan <c>tinjau</c> saat teks berubah,
/// tanpa-operasi saat teks sama, dan lebar kolom semuanya tinggal di
/// <c>Technology.Update</c> — satu tempat, supaya jalan ini tak bisa menyimpang dari
/// aturan yang dijaga uji domain. Irisan ini hanya mengantarkan muatan ke sana lewat
/// jalur bersama <see cref="TopicMutation"/>.
/// <para>
/// Bidang topik <b>tidak</b> diubah: Update menerima <c>fieldId</c>, dan yang dikirim
/// adalah bidang topiknya sendiri.
/// </para>
/// </remarks>
public sealed class UpdateTechnologyHandler(TechnologyDbContext db)
{
    public Task<TopicMutationOutcome> HandleAsync(string slug, UpdateTechnologyRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // PUT mengganti SELURUH ringkasan. Ringkasan kosong bukan "tidak diubah"
        // melainkan mengosongkan bagian Overview — dan klien yang lupa mengirimnya
        // (medan hilang -> null) akan melakukannya tanpa sengaja. Domain menerima
        // ringkasan kosong (topik kurasi memang boleh punya), jadi penolakan ini
        // sengaja hanya di pintu PUT.
        if (string.IsNullOrWhiteSpace(request.Summary))
        {
            return Task.FromResult(TopicMutationOutcome.Invalid(
                "summary",
                "Ringkasan wajib diisi. PUT mengganti seluruh ringkasan; mengosongkan bagian Overview bukan yang dimaksud."));
        }

        return TopicMutation.RunAsync(
            db,
            slug,
            technology => technology.Update(request.Name, request.Summary, technology.FieldId),
            cancellationToken);
    }
}
