using FluentValidation;

namespace TechVerseX.TechnologyService.Features.CreateTechnology;

public sealed class CreateTechnologyValidator : AbstractValidator<CreateTechnologyCommand>
{
    public CreateTechnologyValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nama teknologi wajib diisi.")
            .MaximumLength(200);

        RuleFor(x => x.Summary)
            .MaximumLength(2000).WithMessage("Ringkasan maksimal 2.000 karakter.");

        RuleFor(x => x.FieldSlug)
            .NotEmpty().WithMessage("Bidang wajib diisi - lihat GET /api/v1/fields untuk daftarnya.")
            .MaximumLength(120);

        RuleFor(x => x.Slug)
            .MaximumLength(160)
            .When(x => !string.IsNullOrWhiteSpace(x.Slug));
    }
}
