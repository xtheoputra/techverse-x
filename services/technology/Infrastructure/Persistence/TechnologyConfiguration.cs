using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NpgsqlTypes;
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

        // Pencarian teks penuh — tangga V1 di KERANGKA.md 4.6, dan jawaban
        // ADR-015 bagian 5 atas pencarian hibrida tanpa basis data kedua.
        // Seluruh alasannya di PencarianTeks dan ADR-022.
        builder.Property<NpgsqlTsVector>(PencarianTeks.KolomVektor)
            .HasColumnName(PencarianTeks.NamaKolom)
            .HasColumnType("tsvector")
            .HasComputedColumnSql(PencarianTeks.Ekspresi, stored: true);

        // GIN, bukan GiST: isi TechVerse X jauh lebih sering dibaca daripada
        // ditulis, dan GIN membayar kecepatan bacanya dengan tulisan yang lebih
        // mahal. Pertukaran itu memang yang diinginkan di sini.
        builder.HasIndex(PencarianTeks.KolomVektor)
            .HasMethod("GIN")
            .HasDatabaseName(PencarianTeks.IndeksTechnologies);

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

        // Daftar jenis di CHECK DIBANGKITKAN dari enum, bukan ditulis tangan: daftar
        // tangan di SQL berjalan sendiri begitu enumnya berubah. Karena ia bagian
        // model EF, menambah anggota RelationshipKind membuat model berbeda dari
        // snapshot - ModelMigrasiTests memerah dan `dotnet ef database update`
        // menolak jalan sampai migrasinya dibuat (ADR-023).
        var jenisYangSah = string.Join(", ", Enum.GetNames<RelationshipKind>().Select(n => $"'{n}'"));

        builder.ToTable("technology_relationships", table =>
        {
            // ADR-015 bagian 3 menjanjikan "penolakan simpul berelasi dengan
            // dirinya sendiri". Sampai RelasiAntarTopik janji itu hanya hidup di
            // domain; SQL mentah atau jalan tulis kedua bisa melewatinya.
            table.HasCheckConstraint(
                "ck_technology_relationships_bukan_diri_sendiri",
                "\"FromTechnologyId\" <> \"ToTechnologyId\"");

            // Baris berjenis yang sudah dibuang ditolak saat DISISIPKAN. Tanpa ini ia
            // masuk, lalu setiap pemuatan topiknya melempar - diukur tanpa CHECK:
            // Include(Relationships) atas baris 'Uses' gagal "Cannot convert string
            // value 'Uses' from the database to any value in the mapped
            // 'RelationshipKind' enum", di permintaan BACA, jauh dari penulisnya.
            table.HasCheckConstraint(
                "ck_technology_relationships_kind",
                $"\"Kind\" IN ({jenisYangSah})");
        });

        builder.HasKey(r => r.Id);

        // Kunci dibuat domain (Guid.CreateVersion7), bukan basis data. Tanpa baris
        // ini sisi yang ditambahkan ke topik yang SUDAH TERSIMPAN diterbitkan EF
        // sebagai UPDATE, bukan INSERT -> DbUpdateConcurrencyException -> 500 di
        // permintaan tulis pertama. Kisah lengkapnya di ringkasan
        // ContentSectionConfiguration, tempat jebakan yang sama pertama ketahuan.
        builder.Property(r => r.Id).ValueGeneratedNever();

        builder.Property(r => r.Kind).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(r => r.CreatedAt).IsRequired();

        // Ujung ASAL sudah dipetakan dari sisi Technology (HasMany Relationships,
        // Cascade): sisi keluar sebuah topik memang tidak berarti tanpa topiknya.
        //
        // 🔴 Ujung TUJUAN tidak punya kunci asing sama sekali sampai RelasiAntarTopik -
        // sisi bisa menunjuk topik yang tidak ada, dan pembaca yang JOIN ke
        // technologies akan menjatuhkannya diam-diam. Restrict, bukan Cascade, sama
        // alasannya dengan Field dan Tool: membuang satu topik tidak boleh
        // diam-diam menghapus "Pelajari lebih dulu" dari halaman topik LAIN, yang
        // mungkin sudah diperiksa manusia. Sisi masuknya harus dilepas lebih dulu.
        builder.HasOne<Technology>()
            .WithMany()
            .HasForeignKey(r => r.ToTechnologyId)
            .OnDelete(DeleteBehavior.Restrict);

        // PostgreSQL tidak mengindeks kolom kunci asing sendiri. Ujung asal sudah
        // tertutup indeks unik di bawah (kolom pertamanya From); yang butuh indeks
        // sendiri tinggal ujung tujuan - dipakai pembaca "dibutuhkan oleh".
        builder.HasIndex(r => r.ToTechnologyId).HasDatabaseName("ix_technology_relationships_to_technology_id");

        // Sisi yang sama tidak boleh tercatat dua kali - indeks ini yang
        // menjaganya. Yang TIDAK bisa ia jaga: kebalikannya (B butuh A saat A
        // sudah butuh B), sebab kuncinya per arah. Aturan dua arah itu hidup di
        // Technology.RequireTopic; kunci asing di kedua ujung dan kedua CHECK di
        // atas adalah lapis basis datanya.
        builder.HasIndex(r => new { r.FromTechnologyId, r.ToTechnologyId, r.Kind })
            .IsUnique()
            .HasDatabaseName("ix_technology_relationships_edge");
    }
}
