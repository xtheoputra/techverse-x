using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TechVerseX.TechnologyService.Features.CreateTechnology;
using TechVerseX.TechnologyService.Features.CreateTool;
using TechVerseX.TechnologyService.Features.EditContentSections;
using TechVerseX.TechnologyService.Features.GetTechnology;
using TechVerseX.TechnologyService.Features.ListFields;
using TechVerseX.TechnologyService.Features.Search;
using TechVerseX.TechnologyService.Features.SearchTechnology;
using TechVerseX.TechnologyService.Infrastructure.Persistence;

namespace TechVerseX.TechnologyService;

/// <summary>
/// Titik pasang bounded context Technology ke sebuah host.
/// </summary>
/// <remarks>
/// KERANGKA.md 4.5 menegaskan <em>"logical microservices ≠ harus langsung
/// physical microservices"</em>. Karena itu layanan ini class library yang
/// dipasang ke host <c>apps/api</c>, bukan proses sendiri. Memecahnya jadi
/// proses terpisah nanti tinggal memindahkan pemanggilan modul ini.
/// </remarks>
public static class TechnologyModule
{
    public static IServiceCollection AddTechnologyService(this IServiceCollection services, string connectionString)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        // Platform terkelola menyerahkan kredensial sebagai URI postgres://...,
        // sementara Npgsql menuntut bentuk kunci-nilai. Diterjemahkan DI SINI,
        // bukan di pemanggil, supaya bundel migrasi EF - yang memakai apps/api
        // sebagai startup project - mendapat perlakuan yang sama persis.
        var npgsqlConnectionString = PostgresConnectionString.Normalize(connectionString);

        services.AddDbContext<TechnologyDbContext>(options =>
            options.UseNpgsql(npgsqlConnectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__ef_migrations_history", TechnologyDbContext.Schema)));

        services.AddScoped<CreateTechnologyHandler>();
        services.AddScoped<ListFieldsHandler>();
        services.AddScoped<GetTechnologyHandler>();
        services.AddScoped<SearchTechnologyHandler>();
        services.AddScoped<SearchHandler>();
        services.AddScoped<EditContentSectionsHandler>();
        services.AddScoped<CreateToolHandler>();

        services.AddScoped<IValidator<CreateTechnologyCommand>, CreateTechnologyValidator>();

        return services;
    }

    /// <summary>
    /// Memasang rute bounded context ini. <paramref name="editorialWrites"/>
    /// menentukan apakah permukaan TULIS ikut dipasang.
    /// </summary>
    /// <param name="routes">Host tempat rute dipasang.</param>
    /// <param name="editorialWrites">
    /// <b>Sengaja tidak punya nilai bawaan.</b> Tiap host harus menyatakan
    /// pilihannya; lupa menyebutkannya jadi galat kompilasi, bukan permukaan
    /// tulis yang diam-diam terbuka.
    /// </param>
    /// <remarks>
    /// 🔴 <b>V1 tidak punya autentikasi sama sekali (ADR-013), dan sampai
    /// 2026-09-09 itu berarti SIAPA PUN di internet boleh menulis.</b> ADR-013
    /// menimbang login untuk PEMBACA — Progress Tracker, Badge, AI Mentor — dan
    /// tidak pernah menimbang permukaan tulis REDAKSI. Begitu API tayang di URL
    /// publik (issue #39/#40), <c>POST /api/v1/technologies</c> dan keenam
    /// endpoint bagian isi menerima muatan dari siapa saja: topik baru yang
    /// langsung muncul di halaman muka, tautan asing di bagian Resources, dan
    /// halaman yang dinaikkan ke <c>draf</c> tanpa ada yang memintanya.
    /// <para>
    /// Jalan keluarnya sengaja BUKAN autentikasi buatan sendiri — ADR-013 sudah
    /// menolak itu — melainkan <b>tidak memasang rutenya sama sekali</b>.
    /// Bentuknya meniru Redis di <c>Program.cs</c>: <em>tidak dikonfigurasi =
    /// tidak dipasang</em>, bukan "dipasang lalu ditolak". Rute yang tidak ada
    /// tidak punya rahasia untuk bocor dan tidak perlu dirotasi.
    /// </para>
    /// Lihat <see href="../../docs/adr/ADR-020-permukaan-tulis-api.md">ADR-020</see>.
    /// </remarks>
    public static IEndpointRouteBuilder MapTechnologyEndpoints(this IEndpointRouteBuilder routes, bool editorialWrites)
    {
        ArgumentNullException.ThrowIfNull(routes);

        // Rute publik persis seperti KERANGKA.md 4.9.
        var technologies = routes.MapGroup("/api/v1/technologies").WithTags("Technology");

        technologies.MapSearchTechnology();
        technologies.MapGetTechnology();

        // Bidang punya grupnya sendiri, bukan sub-rute teknologi: menurut ADR-009
        // ia entitas ber-URL kanonik, bukan atribut sebuah teknologi.
        var fields = routes.MapGroup("/api/v1/fields").WithTags("Field");

        fields.MapListFields();

        // Pencarian punya grupnya sendiri: jawabannya memuat BIDANG dan TOPIK
        // sekaligus, jadi menaruhnya di bawah salah satunya akan menyiratkan
        // kepemilikan yang tidak ada.
        var search = routes.MapGroup("/api/v1/search").WithTags("Search");

        search.MapSearch();

        // ---- Batas antara MEMBACA dan MENULIS -------------------------------
        // Segala yang di atas melayani situsnya; segala yang di bawah mengubah
        // isinya. Bentuk produksi V1 adalah terbitan yang hanya bisa dibaca.
        if (!editorialWrites)
        {
            return routes;
        }

        technologies.MapCreateTechnology();

        // Bagian isi halaman bersarang di bawah topiknya — mereka tidak punya
        // hidup di luar topik itu, sama seperti mereka tidak punya DbSet sendiri.
        technologies.MapEditContentSections();

        // Katalog alat punya grup sendiri, BUKAN sub-rute topik: satu alat dipakai
        // banyak topik, dan menempatkannya di bawah salah satunya akan menyiratkan
        // kepemilikan yang tidak ada.
        //
        // Seluruh grup ini permukaan tulis, jadi ia ikut hilang bersama yang lain.
        var tools = routes.MapGroup("/api/v1/tools").WithTags("Tool");

        tools.MapCreateTool();

        return routes;
    }
}
