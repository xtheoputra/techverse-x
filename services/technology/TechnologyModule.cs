using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TechVerseX.TechnologyService.Features.CreateTechnology;
using TechVerseX.TechnologyService.Features.GetTechnology;
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

        services.AddDbContext<TechnologyDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__ef_migrations_history", TechnologyDbContext.Schema)));

        services.AddScoped<CreateTechnologyHandler>();
        services.AddScoped<GetTechnologyHandler>();
        services.AddScoped<SearchTechnologyHandler>();

        services.AddScoped<IValidator<CreateTechnologyCommand>, CreateTechnologyValidator>();

        return services;
    }

    public static IEndpointRouteBuilder MapTechnologyEndpoints(this IEndpointRouteBuilder routes)
    {
        ArgumentNullException.ThrowIfNull(routes);

        // Rute publik persis seperti KERANGKA.md 4.9.
        var group = routes.MapGroup("/api/v1/technologies").WithTags("Technology");

        group.MapSearchTechnology();
        group.MapGetTechnology();
        group.MapCreateTechnology();

        return routes;
    }
}
