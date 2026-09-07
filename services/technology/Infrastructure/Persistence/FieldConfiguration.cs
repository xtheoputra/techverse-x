using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechVerseX.TechnologyService.Domain;

namespace TechVerseX.TechnologyService.Infrastructure.Persistence;

internal sealed class FieldConfiguration : IEntityTypeConfiguration<Field>
{
    public void Configure(EntityTypeBuilder<Field> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("fields");
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Slug).HasMaxLength(120).IsRequired();
        builder.Property(f => f.Name).HasMaxLength(120).IsRequired();
        builder.Property(f => f.Summary).HasMaxLength(1000);
        builder.Property(f => f.DisplayOrder).IsRequired();

        // Enum sebagai teks, sama alasannya dengan TechnologyStatus: dump basis
        // data harus terbaca manusia, dan menyisipkan anggota enum baru tidak
        // boleh diam-diam mengubah arti baris lama.
        builder.Property(f => f.Priority).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.HasIndex(f => f.Slug).IsUnique().HasDatabaseName("ix_fields_slug");
        builder.HasIndex(f => f.Name).IsUnique().HasDatabaseName("ix_fields_name");
        builder.HasIndex(f => f.DisplayOrder).IsUnique().HasDatabaseName("ix_fields_display_order");

        // Daftar bidang disemai migrasi, bukan lewat API. Bidang adalah bagian
        // dari keputusan struktur (ADR-010) — menambah atau membuang satu bidang
        // harus lewat ADR dan migrasi, bukan lewat POST yang tidak meninggalkan
        // jejak keputusan.
        builder.HasData(FieldCatalog.All);
    }
}
