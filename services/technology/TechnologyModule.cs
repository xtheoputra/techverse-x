using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TechVerseX.TechnologyService.Features.CreateTechnology;
using TechVerseX.TechnologyService.Features.GetTechnology;
using TechVerseX.TechnologyService.Features.ListFields;
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

        services.AddScoped<IValidator<CreateTechnologyCommand>, CreateTechnologyValidator>();

        return services;
    }

    public static IEndpointRouteBuilder MapTechnologyEndpoints(this IEndpointRouteBuilder routes)
    {
        ArgumentNullException.ThrowIfNull(routes);

        // Rute publik persis seperti KERANGKA.md 4.9.
        var technologies = routes.MapGroup("/api/v1/technologies").WithTags("Technology");

        technologies.MapSearchTechnology();
        technologies.MapGetTechnology();
        technologies.MapCreateTechnology();

        // Bidang punya grupnya sendiri, bukan sub-rute teknologi: menurut ADR-009
        // ia entitas ber-URL kanonik, bukan atribut sebuah teknologi.
        var fields = routes.MapGroup("/api/v1/fields").WithTags("Field");

        fields.MapListFields();

        return routes;
    }
}
