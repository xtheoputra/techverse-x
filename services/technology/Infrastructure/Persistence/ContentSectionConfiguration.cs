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
/// <para>
/// 🔴 <b><c>ValueGeneratedNever()</c> pada ketiga kunci di bawah BUKAN kerapian —
/// tanpanya, menambah bagian ke topik yang SUDAH TERSIMPAN gagal dengan 500.</b>
/// Kunci di sini dibuat domain (<c>Guid.CreateVersion7()</c>), jadi nilainya sudah
/// terisi saat entitas baru muncul di koleksi agregat. Konvensi EF untuk kunci
/// <c>Guid</c> adalah <c>ValueGeneratedOnAdd</c>, dan dengan itu EF memakai
/// "kuncinya sudah berisi ⇒ barisnya sudah ada" lalu menerbitkan <c>UPDATE</c>
/// alih-alih <c>INSERT</c>. UPDATE itu tidak menyentuh satu baris pun dan muncul
/// sebagai <c>DbUpdateConcurrencyException</c> — pesan yang menuduh balapan data,
/// padahal yang salah pemetaan.
/// </para>
/// <para>
/// ⚠️ <b>Kenapa ini tidak pernah ketahuan sampai Sesi 10:</b> satu-satunya kode
/// yang pernah menyimpan bagian isi membuat topiknya <em>dan</em> bagiannya dalam
/// satu <c>SaveChanges</c>. Di graf yang seluruhnya <c>Added</c>, anaknya ikut
/// <c>Added</c> dan heuristik itu tidak pernah dipakai. Endpoint isi halaman
/// adalah jalur pertama yang menambah anak ke agregat yang sudah ada di basis
/// data. <c>ContentSectionEndpointTests</c> yang menjaganya sekarang.
/// </para>
/// </remarks>
internal sealed class RoadmapStepConfiguration : IEntityTypeConfiguration<RoadmapStep>
{
    public void Configure(EntityTypeBuilder<RoadmapStep> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("roadmap_steps");
        builder.HasKey(s => s.Id);

        // Lihat catatan di ringkasan berkas ini sebelum membuang baris ini.
        builder.Property(s => s.Id).ValueGeneratedNever();

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

        // Tidak ada kueri aplikasi yang memakai indeks ini - arah alat -> topik
        // memang tidak pernah dibaca (#68). Pemakainya pemeriksaan Restrict di atas:
        // menghapus satu alat mencari barisnya di sini per ToolId, dan EXPLAIN
        // kueri pemeriksaan itu memilih indeks ini. Membuangnya pun tidak bisa:
        // diukur 2026-09-28, EF membuatnya lagi sebagai IX_technology_tools_ToolId.
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
        // Kunci dibuat domain, bukan basis data - lihat ringkasan berkas ini.
        builder.Property(p => p.Id).ValueGeneratedNever();

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
        // Kunci dibuat domain, bukan basis data - lihat ringkasan berkas ini.
        builder.Property(r => r.Id).ValueGeneratedNever();

        // Enum jadi teks, alasan yang sama dengan Status dan Maturity: dump basis
        // data harus terbaca manusia, dan menyisipkan anggota enum baru tidak
        // boleh diam-diam mengubah arti baris lama.
        builder.Property(r => r.Type).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(r => r.Title).HasMaxLength(300).IsRequired();
        builder.Property(r => r.Url).HasMaxLength(1000).IsRequired();
        builder.Property(r => r.CreatedAt).IsRequired();

        // Satu-satunya indeks berawalan TechnologyId di tabel ini, jadi ia yang
        // melayani Include(Resources) dan cascade dari technologies - EXPLAIN
        // memilihnya untuk WHERE "TechnologyId" = ... (diukur 2026-09-28, #68).
        // Kolom Type di dalamnya memang tidak dipakai kueri mana pun: urutan per
        // jenis dikerjakan di memori (TechnologyResponseFactory). Tanpa baris ini EF
        // menggantinya dengan IX_resources_TechnologyId - lebih sempit, tapi migrasi
        // demi selisih yang tidak terukur di skala ini.
        builder.HasIndex(r => new { r.TechnologyId, r.Type })
            .HasDatabaseName("ix_resources_technology_type");
    }
}
