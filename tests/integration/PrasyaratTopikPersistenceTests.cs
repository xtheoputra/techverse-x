using System.Data.Common;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TechVerseX.TechnologyService.Domain;
using TechVerseX.TechnologyService.Infrastructure.Persistence;

namespace TechVerseX.Api.Tests;

/// <summary>
/// Lapis basis data knowledge graph (ADR-023): kunci asing di ujung TUJUAN
/// (Restrict), CHECK sisi ke diri sendiri, dan CHECK jenis yang dibangkitkan dari
/// enum — ketiganya datang bersama migrasi <c>RelasiAntarTopik</c>.
/// </summary>
/// <remarks>
/// 🔑 <b>Sengaja menembus agregat lewat SQL mentah.</b> Lewat
/// <see cref="Technology.RequireTopic"/> keadaan-keadaan ini memang tidak bisa
/// dibuat; yang diuji di sini penjaga LAPIS KEDUA — yang tetap berlaku untuk
/// migrasi data, SQL tangan, atau jalan tulis yang suatu saat lupa memanggil
/// agregatnya. Pola yang sama dengan
/// <c>ContentSectionPersistenceTests.Indeks_unik_menolak_langkah_kedua_bernomor_sama</c>:
/// nama constraint-nya harus muncul di galatnya, dan kedua topik BENAR-BENAR ada
/// lebih dulu, supaya yang menolak adalah constraint yang dimaksud — bukan kunci
/// asing yang kebetulan lain.
/// <para>
/// 🔴 <b>Dibuktikan merah lewat migrasi SUNGGUHAN, bukan dengan menyunting
/// constraint:</b> <c>dotnet ef database update PencarianTeksPenuh</c> menjalankan
/// Down <c>RelasiAntarTopik</c>, keempat uji di sini memerah, lalu
/// <c>run.ps1 migrate</c> menghijaukannya lagi. Sekaligus bukti Down-nya bekerja.
/// </para>
/// <para>
/// ⚠️ <b>Urutan pembersihan penting.</b> Ujung tujuan Restrict, jadi sisi harus
/// dihapus SEBELUM topiknya — kalau tidak, <see cref="DisposeAsync"/> sendiri yang
/// ditolak basis data dan sampahnya tertinggal di basis data pengembang. Ia juga
/// harus tetap bersih di keadaan Down, tempat sisi yang menggantung memang bisa
/// tercipta.
/// </para>
/// <para>
/// ⚠️ Menyentuh PostgreSQL sungguhan (<c>run.ps1 up</c> lalu <c>run.ps1 migrate</c>
/// di lokal, service container di CI).
/// </para>
/// </remarks>
public sealed class PrasyaratTopikPersistenceTests : IAsyncLifetime
{
    private const string Postgres =
        "Host=localhost;Port=5432;Database=techversex;Username=techversex;Password=techversex_dev";

    private readonly List<string> _topikDibuat = [];

    /// <summary>
    /// Tanpa <c>Editorial:WritesEnabled</c>: berkas ini tidak memanggil satu
    /// endpoint pun, ia bicara langsung ke <see cref="TechnologyDbContext"/>.
    /// </summary>
    private static WebApplicationFactory<Program> Host() =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:Postgres", Postgres);
            builder.UseSetting("ConnectionStrings:Redis", string.Empty);
        });

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        if (_topikDibuat.Count == 0)
        {
            return;
        }

        using var host = Host();
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TechnologyDbContext>();

        // Sisi DULU, keduanya arah: ujung tujuan Restrict.
        //
        // 🐞 Interpolasi, BUKAN ExecuteSqlRawAsync(sql, slugs.ToArray()). Diukur:
        // string[] kovarian ke params object[], jadi larik itu diperlakukan sebagai
        // DAFTAR PARAMETER, {0} terikat ke slug pertama saja, dan PostgreSQL menolak
        // "op ANY/ALL (array) requires array on right side" — di DisposeAsync, setelah
        // keempat uji hijau, meninggalkan lima topik dan satu sisi di basis data.
        var slugs = _topikDibuat.ToArray();
        await db.Database.ExecuteSqlAsync(
            $"""
            DELETE FROM technology.technology_relationships r
            USING technology.technologies t
            WHERE t."Slug" = ANY({slugs})
              AND (r."FromTechnologyId" = t."Id" OR r."ToTechnologyId" = t."Id");
            """);

        var topik = await db.Technologies.Where(t => _topikDibuat.Contains(t.Slug)).ToListAsync();
        db.Technologies.RemoveRange(topik);
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task Ujung_tujuan_harus_ada()
    {
        using var host = Host();
        var a = await BuatTopikAsync(host, "asal");

        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TechnologyDbContext>();

        // Asal nyata, tujuan uuid yang tidak dimiliki topik mana pun. Sebelum
        // RelasiAntarTopik baris ini MASUK, dan pembaca yang JOIN ke technologies
        // menjatuhkannya tanpa sepatah kata.
        var galat = await Assert.ThrowsAnyAsync<DbException>(() =>
            db.Database.ExecuteSqlRawAsync(
                """
                INSERT INTO technology.technology_relationships ("Id", "FromTechnologyId", "ToTechnologyId", "Kind", "CreatedAt")
                VALUES (gen_random_uuid(), {0}, gen_random_uuid(), 'Requires', now());
                """,
                a));

        Assert.Contains("FK_technology_relationships_technologies_ToTechnologyId", galat.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Topik_yang_masih_dibutuhkan_tidak_bisa_dihapus()
    {
        using var host = Host();
        var a = await BuatTopikAsync(host, "butuh");
        var b = await BuatTopikAsync(host, "dibutuhkan");

        // Sisinya dibuat lewat AGREGAT, di scope baru, ke topik yang SUDAH
        // tersimpan — jalan yang sama dengan endpoint tulisnya nanti. Itu
        // sekaligus menjalankan ValueGeneratedNever pada kunci relasi: tanpanya
        // SaveChanges di bawah menerbitkan UPDATE dan gagal dengan
        // DbUpdateConcurrencyException sebelum satu pun assert tercapai.
        using (var scope = host.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TechnologyDbContext>();

            var syaratB = await db.Technologies
                .AsNoTracking()
                .Where(t => t.Id == b)
                .SelectMany(t => t.Relationships)
                .Where(r => r.Kind == RelationshipKind.Requires)
                .Select(r => r.ToTechnologyId)
                .ToListAsync();

            var topikA = await db.Technologies.Include(t => t.Relationships).SingleAsync(t => t.Id == a);
            topikA.RequireTopic(b, syaratB);
            await db.SaveChangesAsync();
        }

        using (var scope = host.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TechnologyDbContext>();
            Assert.Equal(1, await HitungSisiDariAsync(db, a));

            // Membuang B akan diam-diam mengosongkan "Pelajari lebih dulu" di
            // halaman A kalau ujung tujuannya Cascade — atau meninggalkan sisi
            // yang menggantung kalau tidak ada kunci asingnya sama sekali.
            var galat = await Assert.ThrowsAnyAsync<DbException>(() =>
                db.Database.ExecuteSqlRawAsync("""DELETE FROM technology.technologies WHERE "Id" = {0};""", b));

            Assert.Contains("FK_technology_relationships_technologies_ToTechnologyId", galat.ToString(), StringComparison.Ordinal);
            Assert.Equal(1, await HitungSisiDariAsync(db, a));

            // Kendali arah berlawanan: ujung ASAL tetap Cascade. Membuang A
            // berhasil, dan sisi keluarnya ikut hilang — ia tidak berarti tanpa A.
            await db.Database.ExecuteSqlRawAsync("""DELETE FROM technology.technologies WHERE "Id" = {0};""", a);
            Assert.Equal(0, await HitungSisiDariAsync(db, a));
        }
    }

    [Fact]
    public async Task Sisi_ke_diri_sendiri_ditolak_basis_data()
    {
        // ADR-015 bagian 3 menjanjikan penolakan ini sejak awal; sampai
        // RelasiAntarTopik yang menepatinya hanya domain.
        using var host = Host();
        var a = await BuatTopikAsync(host, "diri");

        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TechnologyDbContext>();

        var galat = await Assert.ThrowsAnyAsync<DbException>(() => SisipkanSisiAsync(db, a, a, "Requires"));

        Assert.Contains("ck_technology_relationships_bukan_diri_sendiri", galat.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Jenis_di_luar_enum_ditolak_basis_data()
    {
        using var host = Host();
        var a = await BuatTopikAsync(host, "jenis-asal");
        var b = await BuatTopikAsync(host, "jenis-tujuan");

        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TechnologyDbContext>();

        // 'Uses' pernah jadi anggota RelationshipKind dan dibuang ADR-023. Baris
        // seperti ini harus ditolak saat DISISIPKAN — kalau lolos, setiap pemuatan
        // topik asalnya melempar "Cannot convert string value 'Uses' ... to any
        // value in the mapped 'RelationshipKind' enum" (diukur tanpa CHECK-nya).
        var galat = await Assert.ThrowsAnyAsync<DbException>(() => SisipkanSisiAsync(db, a, b, "Uses"));

        Assert.Contains("ck_technology_relationships_kind", galat.ToString(), StringComparison.Ordinal);

        // Kendali: sisipan yang SAMA PERSIS dengan jenis yang sah, untuk pasangan
        // nyata yang sama, masuk. Tanpa ini penolakan di atas bisa saja karena
        // bentuk INSERT-nya yang keliru.
        await SisipkanSisiAsync(db, a, b, "Requires");
        Assert.Equal(1, await HitungSisiDariAsync(db, a));
    }

    /// <summary>
    /// Membuat satu topik dalam <c>SaveChanges</c>-nya sendiri, supaya setiap sisi
    /// selalu ditambahkan ke topik yang sudah tersimpan. Slug didaftarkan untuk
    /// dibersihkan SEBELUM disimpan — kalau penyimpanannya yang gagal, membuang
    /// slug yang tidak pernah ada tidak berbiaya apa-apa.
    /// </summary>
    private async Task<Guid> BuatTopikAsync(WebApplicationFactory<Program> host, string peran)
    {
        var slug = $"uji-prasyarat-{peran}-{Guid.CreateVersion7():N}";
        _topikDibuat.Add(slug);

        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TechnologyDbContext>();

        var topik = Technology.Create($"Uji Prasyarat {peran}", "Ringkasan untuk uji.", FieldCatalog.All[0].Id, slug);
        db.Technologies.Add(topik);
        await db.SaveChangesAsync();
        return topik.Id;
    }

    private static Task<int> SisipkanSisiAsync(TechnologyDbContext db, Guid dari, Guid ke, string jenis) =>
        db.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO technology.technology_relationships ("Id", "FromTechnologyId", "ToTechnologyId", "Kind", "CreatedAt")
            VALUES (gen_random_uuid(), {0}, {1}, {2}, now());
            """,
            dari,
            ke,
            jenis);

    /// <summary>
    /// Dihitung lewat SQL mentah, bukan lewat navigasi agregat: setelah topiknya
    /// dibuang, navigasi selalu menjawab nol — termasuk saat barisnya masih ada.
    /// </summary>
    private static Task<int> HitungSisiDariAsync(TechnologyDbContext db, Guid dari) =>
        db.Database
            .SqlQuery<int>($"""SELECT count(*)::int AS "Value" FROM technology.technology_relationships WHERE "FromTechnologyId" = {dari}""")
            .SingleAsync();
}
