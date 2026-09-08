using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechVerseX.TechnologyService.Domain;

namespace TechVerseX.TechnologyService.Infrastructure.Persistence;

/// <summary>
/// Pemetaan keempat bagian isi template ADR-012 ke tabelnya masing-masing.
/// </summary>
/// <remarks>
/// ADR-015 menolak <c>content_block</c> generik dengan kolom <c>type</c> dan
/// <c>body</c>: bentuk seperti itu memindahkan seluruh aturan ke kode aplikasi
/// dan membuat "halaman ini lengkap atau belum" jadi pertanyaan yang tidak bisa
/// dijawab basis data. Berkas ini konsekuensinya — empat tabel bertipe.
/// </remarks>
internal sealed class RoadmapStepConfiguration : IEntityTypeConfiguration<RoadmapStep>
{
    public void Configure(EntityTypeBuilder<RoadmapStep> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("roadmap_steps");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Order).IsRequired();
        builder.Property(s => s.Title).HasMaxLength(200).IsRequired();
        builder.Property(s => s.Description).HasMaxLength(2000);
        builder.Property(s => s.CreatedAt).IsRequired();

        // Dua langkah bernomor sama di satu topik berarti roadmap yang urutannya
        // ambigu. Agregat sudah mencegahnya dengan memberi nomor sendiri; indeks
        // ini yang membuat pencegahan itu tetap berlaku kalau suatu saat ada
        // jalan tulis lain - migrasi massal, misalnya.
        builder.HasIndex(s => new { s.TechnologyId, s.Order })
            .IsUnique()
            .HasDatabaseName("ix_roadmap_steps_technology_order");

        // IsPrerequisite diturunkan dari Order, jadi ia bukan kolom.
        builder.Ignore(s => s.IsPrerequisite);
    }
}

internal sealed class ToolConfiguration : IEntityTypeConfiguration<Tool>
{
    public void Configure(EntityTypeBuilder<Tool> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("tools");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Slug).HasMaxLength(Tool.MaxSlugLength).IsRequired();
        builder.Property(t => t.Name).HasMaxLength(200).IsRequired();
        builder.Property(t => t.Summary).HasMaxLength(2000);
        builder.Property(t => t.Homepage).HasMaxLength(500);
        builder.Property(t => t.CreatedAt).IsRequired();

        builder.HasIndex(t => t.Slug).IsUnique().HasDatabaseName("ix_tools_slug");
    }
}

internal sealed class TechnologyToolConfiguration : IEntityTypeConfiguration<TechnologyTool>
{
    public void Configure(EntityTypeBuilder<TechnologyTool> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("technology_tools");

        // Kunci gabungan: satu alat tercatat sekali per topik, dan itu ditegakkan
        // kunci primernya sendiri - bukan indeks unik tambahan.
        builder.HasKey(t => new { t.TechnologyId, t.ToolId });

        builder.Property(t => t.Note).HasMaxLength(500);
        builder.Property(t => t.CreatedAt).IsRequired();

        // Restrict di sisi Tool, sama alasannya dengan Field: membuang satu alat
        // tidak boleh diam-diam mengosongkan bagian Tools di halaman mana pun.
        // Alat yang benar-benar dihapus harus dilepas dari topiknya lebih dulu.
        builder.HasOne<Tool>()
            .WithMany()
            .HasForeignKey(t => t.ToolId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(t => t.ToolId).HasDatabaseName("ix_technology_tools_tool_id");
    }
}

internal sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("projects");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Title).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Brief).HasMaxLength(4000).IsRequired();
        builder.Property(p => p.CreatedAt).IsRequired();
    }
}

internal sealed class ResourceConfiguration : IEntityTypeConfiguration<Resource>
{
    public void Configure(EntityTypeBuilder<Resource> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("resources");
        builder.HasKey(r => r.Id);

        // Enum jadi teks, alasan yang sama dengan Status dan Maturity: dump basis
        // data harus terbaca manusia, dan menyisipkan anggota enum baru tidak
        // boleh diam-diam mengubah arti baris lama.
        builder.Property(r => r.Type).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(r => r.Title).HasMaxLength(300).IsRequired();
        builder.Property(r => r.Url).HasMaxLength(1000).IsRequired();
        builder.Property(r => r.CreatedAt).IsRequired();

        builder.HasIndex(r => new { r.TechnologyId, r.Type })
            .HasDatabaseName("ix_resources_technology_type");
    }
}
