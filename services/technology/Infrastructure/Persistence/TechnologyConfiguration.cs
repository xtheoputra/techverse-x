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

        // Angka yang sama dipakai penjaga masukan (CreateTechnologyValidator).
        // Menaikkannya di sini saja tidak cukup - ia butuh migrasi.
        builder.Property(t => t.Slug).HasMaxLength(Technology.MaxSlugLength).IsRequired();
        builder.Property(t => t.Name).HasMaxLength(200).IsRequired();
        builder.Property(t => t.Summary).HasMaxLength(2000);

        // Enum disimpan sebagai teks, bukan angka: dump basis data harus bisa
        // dibaca manusia, dan menyisipkan anggota enum baru tidak boleh
        // diam-diam mengubah arti baris lama.
        builder.Property(t => t.Status).HasConversion<string>().HasMaxLength(20).IsRequired();

        // 🔴 KOLOM TERPISAH, BUKAN GABUNGAN DARI Status. Dua sumbu berbeda —
        // Status menjawab "boleh tayang?", Maturity menjawab "seberapa dipercaya
        // isinya?". Lihat ADR-015 bagian 1; menggabungkannya menghilangkan
        // kemampuan menerbitkan halaman kurasi yang jujur.
        builder.Property(t => t.Maturity).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(t => t.ReviewedBy).HasMaxLength(120);
        builder.Property(t => t.ReviewedAt);

        builder.Property(t => t.CreatedAt).IsRequired();
        builder.Property(t => t.UpdatedAt).IsRequired();

        builder.HasIndex(t => t.Slug).IsUnique().HasDatabaseName("ix_technologies_slug");
        builder.HasIndex(t => t.FieldId).HasDatabaseName("ix_technologies_field_id");

        // Menyaring "topik apa saja yang sudah diperiksa manusia di bidang ini"
        // adalah kueri yang dipakai untuk mengukur kemajuan proyek
        // (docs/RENCANA-V1.md), jadi ia diberi indeksnya sendiri.
        builder.HasIndex(t => new { t.FieldId, t.Maturity }).HasDatabaseName("ix_technologies_field_maturity");

        // Restrict, bukan Cascade: membuang satu bidang tidak boleh diam-diam
        // ikut membuang topik-topiknya. Kalau sebuah bidang benar-benar dihapus,
        // topiknya harus dipindahkan lebih dulu — dan itu keputusan manusia.
        builder.HasOne<Field>()
            .WithMany()
            .HasForeignKey(t => t.FieldId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(t => t.Relationships)
            .WithOne()
            .HasForeignKey(r => r.FromTechnologyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(t => t.Relationships).UsePropertyAccessMode(PropertyAccessMode.Field);

        // Keempat bagian isi template ADR-012. Cascade, tidak seperti Field dan
        // Tool di atas: langkah roadmap, proyek, dan sumber TIDAK punya arti di
        // luar topiknya, jadi membuang topiknya memang membuang isinya. Yang
        // tidak ikut terbuang adalah Tool-nya sendiri - yang dihapus di sini
        // cuma tautannya.
        //
        // JUJUR SOAL APA YANG BLOK INI LAKUKAN: konvensi EF Core sudah memetakan
        // keempatnya sendiri. Diukur, bukan diduga - seluruh blok ini berikut
        // keempat Navigation di bawahnya pernah DIMATIKAN, dan keenam uji
        // integrasi TETAP HIJAU. Jadi ia menuliskan MAKSUD (terutama pilihan
        // Cascade di atas), bukan menahan sesuatu. Jangan menyangka menghapusnya
        // akan memerahkan uji - itu justru cara kehilangan pilihan Cascade ini
        // tanpa ada yang memberi tahu.
        builder.HasMany(t => t.Roadmap)
            .WithOne()
            .HasForeignKey(s => s.TechnologyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Tools)
            .WithOne()
            .HasForeignKey(t => t.TechnologyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Projects)
            .WithOne()
            .HasForeignKey(p => p.TechnologyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Resources)
            .WithOne()
            .HasForeignKey(r => r.TechnologyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(t => t.Roadmap).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(t => t.Tools).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(t => t.Projects).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(t => t.Resources).UsePropertyAccessMode(PropertyAccessMode.Field);

        // Diturunkan dari keempat koleksi di atas, jadi ia bukan kolom.
        builder.Ignore(t => t.MissingSections);

        // Event domain hidup di memori sampai bus-nya dipasang (ADR-004).
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
