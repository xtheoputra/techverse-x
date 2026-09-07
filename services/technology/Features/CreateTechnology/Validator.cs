using FluentValidation;
using TechVerseX.TechnologyService.Domain;

namespace TechVerseX.TechnologyService.Features.CreateTechnology;

/// <summary>
/// Penjaga muatan <c>POST /api/v1/technologies</c>.
/// </summary>
/// <remarks>
/// 🔴 Validator ini SATU-SATUNYA penjaga sebelum handler: tidak ada satu pun
/// <c>try/catch</c> antara endpoint dan <c>app.UseExceptionHandler()</c>. Karena
/// itu aturannya keras — <b>apa pun yang diluluskan di sini harus bisa dikerjakan
/// sampai selesai.</b> Muatan yang lolos lalu membuat lapisan bawah melempar akan
/// sampai ke pemanggil sebagai 500, padahal yang salah muatannya.
/// </remarks>
public sealed class CreateTechnologyValidator : AbstractValidator<CreateTechnologyCommand>
{
    public CreateTechnologyValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nama teknologi wajib diisi.")
            .MaximumLength(200);

        // Aturan terpisah, BUKAN disambung ke rantai di atas: `When` di ujung rantai
        // FluentValidation berlaku untuk SELURUH rantai itu, jadi menyambungnya akan
        // mematikan `NotEmpty` dan `MaximumLength` setiap kali slug diisi.
        RuleFor(x => x.Name)
            .Must(BisaDipakaiSebagaiSlug)
            .WithMessage("Nama ini tidak menyisakan huruf atau angka yang bisa dipakai sebagai alamat halaman. Pakai nama yang memuat huruf/angka, atau isi 'slug' sendiri.")
            .When(x => string.IsNullOrWhiteSpace(x.Slug));

        RuleFor(x => x.Summary)
            .MaximumLength(2000).WithMessage("Ringkasan maksimal 2.000 karakter.");

        RuleFor(x => x.FieldSlug)
            .NotEmpty().WithMessage("Bidang wajib diisi - lihat GET /api/v1/fields untuk daftarnya.")
            .MaximumLength(120);

        RuleFor(x => x.Slug)
            .MaximumLength(Technology.MaxSlugLength)
            .Must(BisaDipakaiSebagaiSlug)
            .WithMessage("Slug ini tidak menyisakan huruf atau angka yang bisa dipakai sebagai alamat halaman.")
            .When(x => !string.IsNullOrWhiteSpace(x.Slug));
    }

    /// <summary>
    /// Menjawab pertanyaan yang sesungguhnya ditanyakan: apakah nilai ini bisa
    /// diubah jadi slug yang MUAT di kolomnya. Dua kegagalan sekaligus dijaga —
    /// masukan tanpa huruf/angka sama sekali, dan nama panjang yang slug
    /// turunannya melewati <see cref="Technology.MaxSlugLength"/>.
    /// </summary>
    private static bool BisaDipakaiSebagaiSlug(string? value) =>
        Slugs.TryFrom(value, out var slug) && slug.Length <= Technology.MaxSlugLength;
}
