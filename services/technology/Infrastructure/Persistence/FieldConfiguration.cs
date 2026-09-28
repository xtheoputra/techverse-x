using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NpgsqlTypes;
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
        // Tidak ada kueri yang memakai indeks ini - bidang dicari lewat Slug atau
        // Id, dan Name hanya dicocokkan pola tanpa peka huruf (#68). Yang ia jaga
        // KEUNIKAN: diukur 2026-09-28, mengganti nama bidang `xr` jadi "IoT"
        // ditolak "duplicate key value violates unique constraint". Dua kartu
        // bidang bernama sama tidak bisa dibedakan pembaca.
        builder.HasIndex(f => f.Name).IsUnique().HasDatabaseName("ix_fields_name");
        builder.HasIndex(f => f.DisplayOrder).IsUnique().HasDatabaseName("ix_fields_display_order");

        // Bidang ikut dicari, dan itu BUKAN kelengkapan.
        //
        // Selama isinya masih sedikit, bidangnya jauh lebih banyak daripada
        // topiknya, jadi hampir semua yang bisa ditemukan orang di situs ini
        // adalah BIDANG. Pencarian yang cuma menjawab topik akan mengembalikan
        // daftar kosong untuk hampir setiap kata — termasuk kata yang
        // jelas-jelas ada di halaman muka.
        // Terukur: "quantum" tidak menemukan apa pun lewat topik, padahal ada
        // bidang bernama Quantum Computing.
        //
        // Ekspresinya sama persis dengan technologies — dua kolom bernama sama,
        // satu sumber kebenaran di PencarianTeks.
        builder.Property<NpgsqlTsVector>(PencarianTeks.KolomVektor)
            .HasColumnName(PencarianTeks.NamaKolom)
            .HasColumnType("tsvector")
            .HasComputedColumnSql(PencarianTeks.Ekspresi, stored: true);

        builder.HasIndex(PencarianTeks.KolomVektor)
            .HasMethod("GIN")
            .HasDatabaseName(PencarianTeks.IndeksFields);

        // Daftar bidang disemai migrasi, bukan lewat API. Bidang adalah bagian
        // dari keputusan struktur (ADR-010) — menambah atau membuang satu bidang
        // harus lewat ADR dan migrasi, bukan lewat POST yang tidak meninggalkan
        // jejak keputusan.
        builder.HasData(FieldCatalog.All);
    }
}
