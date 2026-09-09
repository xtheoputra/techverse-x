using Microsoft.EntityFrameworkCore;
using TechVerseX.Contracts.Technology;
using TechVerseX.TechnologyService.Domain;
using TechVerseX.TechnologyService.Infrastructure.Persistence;

namespace TechVerseX.TechnologyService.Features.CreateTechnology;

public sealed class CreateTechnologyHandler(TechnologyDbContext db)
{
    public async Task<Result> HandleAsync(CreateTechnologyCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (!Slugs.TryFrom(command.FieldSlug, out var fieldSlug))
        {
            return Result.UnknownField(command.FieldSlug);
        }

        var field = await db.Fields
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Slug == fieldSlug, cancellationToken)
            .ConfigureAwait(false);

        if (field is null)
        {
            return Result.UnknownField(command.FieldSlug);
        }

        var technology = Domain.Technology.Create(command.Name, command.Summary, field.Id, command.Slug);

        var slugTaken = await db.Technologies
            .AnyAsync(t => t.Slug == technology.Slug, cancellationToken)
            .ConfigureAwait(false);

        if (slugTaken)
        {
            return Result.SlugConflict(technology.Slug);
        }

        // 🔴 Bidang dan topik BERBAGI SATU ruang nama URL (ADR-009: "/teknologi/<slug>
        // adalah URL KANONIK tiap bidang dan tiap topik"), tapi keunikannya dijaga
        // DUA indeks yang terpisah - satu di `fields`, satu di `technologies`.
        // Tidak ada satu pun yang membentang di antara keduanya, jadi sampai
        // pemeriksaan ini ada, sebuah topik boleh mengambil slug milik bidang dan
        // membuat alamat yang ADR-009 janjikan stabil jadi ambigu.
        //
        // Diperiksa DI SINI, bukan di basis data, karena Postgres tidak punya
        // keunikan lintas tabel tanpa trigger. Konsekuensinya jujur: ada celah
        // balapan yang sangat sempit antara pemeriksaan ini dan SaveChanges. Itu
        // celah yang sama yang sudah dipikul pemeriksaan slug di atas - bedanya
        // yang itu masih dijaring indeks unik sebagai jaring terakhir, yang ini
        // tidak. Katalog bidang tertutup dan hanya berubah lewat ADR + migrasi,
        // jadi taruhannya kecil dan disengaja.
        var fieldOwnsSlug = await db.Fields
            .AnyAsync(f => f.Slug == technology.Slug, cancellationToken)
            .ConfigureAwait(false);

        if (fieldOwnsSlug)
        {
            return Result.SlugOwnedByField(technology.Slug);
        }

        db.Technologies.Add(technology);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // Event sudah terkumpul di aggregate; penerbitannya menunggu bus (ADR-004).
        technology.ClearEvents();

        return Result.Created(TechnologyResponseFactory.From(technology, field));
    }

    public sealed record Result(
        TechnologyResponse? Value,
        string? ConflictingSlug,
        string? UnknownFieldSlug,
        string? FieldOwnedSlug)
    {
        public bool IsConflict => ConflictingSlug is not null;

        public bool IsUnknownField => UnknownFieldSlug is not null;

        /// <summary>Slug yang diminta sudah dipegang sebuah BIDANG, bukan topik lain.</summary>
        public bool IsSlugOwnedByField => FieldOwnedSlug is not null;

        public static Result Created(TechnologyResponse value) => new(value, null, null, null);

        public static Result SlugConflict(string slug) => new(null, slug, null, null);

        /// <summary>
        /// Bidang tidak dikenal. Ini 400, bukan 404: yang salah muatan permintaan,
        /// bukan alamat yang diminta. Daftar bidang tertutup dan disemai migrasi
        /// (ADR-010), jadi slug di luar daftar itu memang muatan yang keliru.
        /// </summary>
        public static Result UnknownField(string slug) => new(null, null, slug, null);

        /// <summary>
        /// Slugnya milik sebuah bidang. Ini <b>409, sama seperti tabrakan dengan
        /// topik lain</b> — sebabnya sama persis (alamatnya sudah ada yang punya),
        /// dan yang membedakan hanya siapa pemiliknya. Membalasnya 400 akan
        /// menyiratkan muatannya cacat, padahal ia sah; yang penuh itu ruang
        /// namanya.
        /// </summary>
        public static Result SlugOwnedByField(string slug) => new(null, null, null, slug);
    }
}
