using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TechVerseX.TechnologyService.Infrastructure.Persistence;

namespace TechVerseX.Api.Tests;

/// <summary>
/// Apakah model EF yang dibangun kode hari ini SAMA dengan snapshot migrasi
/// terakhir — atau ada perubahan pemetaan yang lupa dijadikan migrasi.
/// </summary>
/// <remarks>
/// 🔑 <b>Kenapa uji ini ada sekarang.</b> Gerbang CI "Migrasi bisa dijalankan dari
/// nol" mati selama kuota Actions habis (#57), padahal cabang ini menumpuk migrasi
/// kedua (<c>RelasiAntarTopik</c>) di atas migrasi yang belum pernah diluluskan
/// CI. Uji ini penggantinya yang bisa dijalankan di mesin mana pun — dan ia tetap
/// berguna setelah CI hidup lagi, sebab ia menjawab lebih cepat.
/// <para>
/// ⚠️ <b>Batas yang diukur, bukan diklaim.</b> Yang tertangkap hanya perubahan
/// yang mengubah <em>model relasional</em>: kolom, indeks, kunci asing, CHECK.
/// Contoh yang benar-benar dijaga: menambah anggota <c>RelationshipKind</c>
/// mengubah SQL CHECK <c>ck_technology_relationships_kind</c> (dibangkitkan dari
/// enum), jadi uji ini memerah. Sebaliknya membuang
/// <c>ValueGeneratedNever()</c> dari kunci relasi TIDAK memerahkannya — kunci
/// <c>uuid</c> tanpa identitas tidak mengubah kolom apa pun. Penjaga baris itu
/// uji endpoint yang menulis sisi ke topik yang sudah tersimpan, bukan berkas ini.
/// </para>
/// <para>
/// Tidak menyentuh baris apa pun: perbandingannya antara model di memori dan
/// snapshot yang dikompilasi ke rakitan. Host tetap dinyalakan supaya konteks yang
/// diperiksa adalah konteks yang SAMA PERSIS dengan yang dipakai API, lengkap
/// dengan penyedia Npgsql-nya.
/// </para>
/// </remarks>
public sealed class ModelMigrasiTests
{
    private const string Postgres =
        "Host=localhost;Port=5432;Database=techversex;Username=techversex;Password=techversex_dev";

    [Fact]
    public void Model_EF_sama_dengan_snapshot_migrasi_terakhir()
    {
        using var host = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:Postgres", Postgres);
            builder.UseSetting("ConnectionStrings:Redis", string.Empty);
        });

        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TechnologyDbContext>();

        Assert.False(
            db.Database.HasPendingModelChanges(),
            "Pemetaan EF berubah tanpa migrasi. Jalankan `.\\run.ps1 migration <NamaMigrasi>` "
            + "(atau `make migration NAME=<NamaMigrasi>`), periksa Up/Down-nya dengan tangan, "
            + "lalu `.\\run.ps1 db-script`. Tanpa itu `dotnet ef database update` menolak jalan "
            + "(PendingModelChangesWarning) dan CI 'Migrasi bisa dijalankan dari nol' memerah.");
    }
}
