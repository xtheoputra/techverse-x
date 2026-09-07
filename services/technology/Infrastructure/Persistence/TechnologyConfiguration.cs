using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechVerseX.TechnologyService.Domain;

namespace TechVerseX.TechnologyService.Infrastructure.Persistence;

internal sealed class TechnologyConfiguration : IEntityTypeConfiguration<Technology>
{
    public void Configure(EntityTypeBuilder<Technology> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("technologies");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Slug).HasMaxLength(160).IsRequired();
        builder.Property(t => t.Name).HasMaxLength(200).IsRequired();
        builder.Property(t => t.Summary).HasMaxLength(2000);
        builder.Property(t => t.Category).HasMaxLength(120).IsRequired();

        // Enum disimpan sebagai teks, bukan angka: dump basis data harus bisa
        // dibaca manusia, dan menyisipkan anggota enum baru tidak boleh
        // diam-diam mengubah arti baris lama.
        builder.Property(t => t.Status).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.Property(t => t.CreatedAt).IsRequired();
        builder.Property(t => t.UpdatedAt).IsRequired();

        builder.HasIndex(t => t.Slug).IsUnique().HasDatabaseName("ix_technologies_slug");
        builder.HasIndex(t => t.Category).HasDatabaseName("ix_technologies_category");

        builder.HasMany(t => t.Relationships)
            .WithOne()
            .HasForeignKey(r => r.FromTechnologyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(t => t.Relationships).UsePropertyAccessMode(PropertyAccessMode.Field);

        // Event domain hidup di memori sampai bus-nya dipasang (ADR-0005).
        builder.Ignore(t => t.Events);
    }
}

internal sealed class TechnologyRelationshipConfiguration : IEntityTypeConfiguration<TechnologyRelationship>
{
    public void Configure(EntityTypeBuilder<TechnologyRelationship> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("technology_relationships");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Kind).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(r => r.CreatedAt).IsRequired();

        // Sisi yang sama tidak boleh tercatat dua kali.
        builder.HasIndex(r => new { r.FromTechnologyId, r.ToTechnologyId, r.Kind })
            .IsUnique()
            .HasDatabaseName("ix_technology_relationships_edge");
    }
}
