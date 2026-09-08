using System.Data.Common;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TechVerseX.TechnologyService.Domain;
using TechVerseX.TechnologyService.Infrastructure.Persistence;

namespace TechVerseX.Api.Tests;

/// <summary>
/// Apakah keempat bagian isi template ADR-012 benar-benar bertahan melewati
/// PostgreSQL — dan kembali dalam bentuk yang sama.
/// </summary>
/// <remarks>
/// 🔑 Uji unit tidak bisa menjawab ini: koleksi di agregat ini semuanya
/// <b>field privat</b>, dan apakah ia benar-benar kembali terisi dari PostgreSQL
/// hanya terjawab dengan menyimpannya ke PostgreSQL.
/// <para>
/// ⚠️ <b>Batas yang diukur, bukan diklaim.</b> Rancangan awal berkas ini menulis
/// bahwa ia menjaga baris <c>UsePropertyAccessMode(PropertyAccessMode.Field)</c>
/// di <c>TechnologyConfiguration</c>. <b>Itu keliru, dan dibuktikan keliru:</b>
/// keempat baris itu — bahkan keempat blok <c>HasMany</c>-nya sekalian —
/// dimatikan, dan keenam uji di sini <b>tetap hijau</b>. Konvensi EF Core sudah
/// menemukan field pendukungnya sendiri. Konfigurasi eksplisit itu dipertahankan
/// karena ia menuliskan <em>maksud</em> (terutama pilihan Cascade), bukan karena
/// ia menahan sesuatu.
/// </para>
/// <para>
/// Yang benar-benar dijaga berkas ini: bahwa agregat lengkap bisa <b>bolak-balik
/// lewat PostgreSQL sungguhan</b> — kolom <c>"Order"</c> yang kebetulan kata
/// kunci SQL, enum yang disimpan sebagai teks, kunci gabungan
/// <c>technology_tools</c>, dan cascade yang membersihkan keempat bagian saat
/// topiknya dibuang. Uji kedua lebih tajam: ia <b>terbukti</b> memerah pada
/// indeks uniknya, dan pesan galatnya menyebut nama indeks itu.
/// </para>
/// <para>
/// ⚠️ Menyentuh PostgreSQL sungguhan (<c>run.ps1 up</c> di lokal, service
/// container di CI), sama seperti <see cref="HealthEndpointTests"/>.
/// </para>
/// </remarks>
public sealed class ContentSectionPersistenceTests
{
    private const string Postgres =
        "Host=localhost;Port=5432;Database=techversex;Username=techversex;Password=techversex_dev";

    private static WebApplicationFactory<Program> Host() =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:Postgres", Postgres);
            builder.UseSetting("ConnectionStrings:Redis", string.Empty);
        });

    [Fact]
    public async Task Kelima_bagian_bertahan_melewati_PostgreSQL()
    {
        using var host = Host();
        var slug = $"uji-bagian-{Guid.CreateVersion7():N}";
        var toolSlug = $"uji-alat-{Guid.CreateVersion7():N}";
        Guid toolId;

        try
        {
            // ---- tulis ----
            using (var scope = host.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<TechnologyDbContext>();

                var tool = Tool.Create("Alat Uji", "Dipakai uji ini saja.", null, toolSlug);
                db.Tools.Add(tool);
                toolId = tool.Id;

                var technology = Technology.Create("Uji Bagian Isi", "Ringkasan uji.", FieldCatalog.All[0].Id, slug);
                technology.SetPrerequisite("Prasyarat", "Dasar-dasarnya.");
                technology.AddRoadmapStep("Langkah satu", "Menjalankan contoh terkecil.");
                technology.AddRoadmapStep("Langkah dua", "Menambah satu tool.");
                technology.AttachTool(toolId, "Dipakai di langkah dua.");
                technology.AddProject("Proyek kecil", "Dari awal sampai selesai.");
                technology.AddResource(ResourceType.OfficialDocs, "Dokumentasi", "https://example.com/docs");
                technology.AddResource(ResourceType.Paper, "Paper", "https://arxiv.org/abs/1234.5678");

                technology.MarkDrafted();

                db.Technologies.Add(technology);
                await db.SaveChangesAsync();
            }

            // ---- baca kembali, di scope BARU supaya bukan cache yang menjawab ----
            using (var scope = host.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<TechnologyDbContext>();

                var dibaca = await db.Technologies
                    .AsNoTracking()
                    .Include(t => t.Roadmap)
                    .Include(t => t.Tools)
                    .Include(t => t.Projects)
                    .Include(t => t.Resources)
                    .SingleAsync(t => t.Slug == slug);

                // Kalau PropertyAccessMode.Field hilang dari konfigurasi, keempat
                // koleksi ini kosong di sini dan MissingSections menyebut empat
                // bagian — tanpa satu pun galat.
                Assert.Empty(dibaca.MissingSections);
                Assert.Equal(ContentMaturity.MachineDrafted, dibaca.Maturity);

                Assert.Equal([0, 1, 2], dibaca.Roadmap.OrderBy(s => s.Order).Select(s => s.Order));
                Assert.Equal("Prasyarat", dibaca.Roadmap.Single(s => s.IsPrerequisite).Title);

                Assert.Equal(toolId, Assert.Single(dibaca.Tools).ToolId);
                Assert.Single(dibaca.Projects);
                Assert.Equal(2, dibaca.Resources.Count);
                Assert.Contains(dibaca.Resources, r => r.Type == ResourceType.Paper);
            }
        }
        finally
        {
            await BersihkanAsync(host, slug, toolSlug);
        }
    }

    [Fact]
    public async Task Indeks_unik_menolak_langkah_kedua_bernomor_sama()
    {
        // Agregatnya sudah mencegah ini dengan memberi nomor sendiri. Yang diuji
        // di sini penjaga LAPIS KEDUA — yang tetap berlaku kalau suatu saat ada
        // jalan tulis lain, misalnya migrasi data massal.
        //
        // Sengaja menembus agregat lewat SQL mentah, sebab lewat API-nya memang
        // tidak ada cara membuat keadaan ini.
        using var host = Host();
        var slug = $"uji-indeks-{Guid.CreateVersion7():N}";

        try
        {
            Guid technologyId;

            using (var scope = host.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<TechnologyDbContext>();

                // Topiknya HARUS benar-benar ada lebih dulu. Tanpa ini, sisipan di
                // bawah gagal karena kunci asing, dan uji ini akan hijau tanpa
                // pernah menyentuh indeks yang katanya ia jaga.
                var technology = Technology.Create("Uji Indeks", "Ringkasan.", FieldCatalog.All[0].Id, slug);
                technology.SetPrerequisite("Prasyarat", "Dasar.");
                db.Technologies.Add(technology);
                await db.SaveChangesAsync();
                technologyId = technology.Id;
            }

            using (var scope = host.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<TechnologyDbContext>();

                // Langkah 0 sudah ada dari SetPrerequisite di atas. Menyisipkan
                // langkah 0 kedua hanya bisa gagal karena indeks uniknya.
                //
                // DbException, bukan DbUpdateException: ExecuteSqlRawAsync
                // menembus change tracker, jadi yang naik adalah galat provider
                // (PostgresException) apa adanya.
                var galat = await Assert.ThrowsAnyAsync<DbException>(() =>
                    db.Database.ExecuteSqlRawAsync(
                        """
                        INSERT INTO technology.roadmap_steps ("Id", "TechnologyId", "Order", "Title", "Description", "CreatedAt")
                        VALUES (gen_random_uuid(), {0}, 0, 'langkah nol kedua', '', now());
                        """,
                        technologyId));

                Assert.Contains(
                    "ix_roadmap_steps_technology_order",
                    galat.ToString(),
                    StringComparison.Ordinal);
            }
        }
        finally
        {
            await BersihkanAsync(host, slug, toolSlug: null);
        }
    }

    private static async Task BersihkanAsync(WebApplicationFactory<Program> host, string slug, string? toolSlug)
    {
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TechnologyDbContext>();

        // Membuang topiknya ikut membuang keempat bagian isinya lewat cascade —
        // dan itu sekaligus membuktikan cascade-nya terpasang.
        var technology = await db.Technologies.FirstOrDefaultAsync(t => t.Slug == slug).ConfigureAwait(false);
        if (technology is not null)
        {
            db.Technologies.Remove(technology);
            await db.SaveChangesAsync().ConfigureAwait(false);
        }

        if (toolSlug is not null)
        {
            var tool = await db.Tools.FirstOrDefaultAsync(t => t.Slug == toolSlug).ConfigureAwait(false);
            if (tool is not null)
            {
                db.Tools.Remove(tool);
                await db.SaveChangesAsync().ConfigureAwait(false);
            }
        }
    }
}
