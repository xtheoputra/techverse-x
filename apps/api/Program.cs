using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;
using TechVerseX.Api.Platform;
using TechVerseX.TechnologyService;
using TechVerseX.TechnologyService.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// ---- Konfigurasi -----------------------------------------------------------
var postgres = builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException(
        "ConnectionStrings:Postgres belum diisi. Jalankan 'make up' lebih dulu, atau salin .env.example.");

var redisConnection = builder.Configuration.GetConnectionString("Redis")
    ?? throw new InvalidOperationException(
        "ConnectionStrings:Redis belum diisi. Jalankan 'make up' lebih dulu, atau salin .env.example.");

// ---- Logging (T009) --------------------------------------------------------
builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(options =>
{
    options.SingleLine = true;
    options.TimestampFormat = "HH:mm:ss ";
});

// ---- Layanan ---------------------------------------------------------------
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(redisConnection));

builder.Services.AddTechnologyService(postgres);

// ---- Health checks (T010) --------------------------------------------------
// Dipisah dua tag supaya Kubernetes bisa membedakan "proses hidup" dari
// "siap menerima lalu lintas" — lihat KERANGKA.md 4.12.
builder.Services.AddHealthChecks()
    .AddDbContextCheck<TechnologyDbContext>(
        name: "postgres",
        failureStatus: HealthStatus.Unhealthy,
        tags: ["ready"])
    .AddCheck<RedisHealthCheck>(
        name: "redis",
        failureStatus: HealthStatus.Unhealthy,
        tags: ["ready"]);

var app = builder.Build();

// ---- Pipeline --------------------------------------------------------------
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Liveness: apakah prosesnya sendiri masih hidup. TIDAK menyentuh dependensi,
// supaya Postgres yang sedang gagal tidak memicu restart yang percuma.
app.MapHealthChecks("/health/live", new()
{
    Predicate = _ => false,
});

// Readiness: apakah kita siap menerima lalu lintas — ini yang menyentuh
// Postgres dan Redis sungguhan.
app.MapHealthChecks("/health/ready", new()
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = static async (context, report) =>
    {
        context.Response.ContentType = "application/json; charset=utf-8";

        var payload = new
        {
            status = report.Status.ToString(),
            durationMs = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description,
                durationMs = entry.Value.Duration.TotalMilliseconds,
            }),
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload)).ConfigureAwait(false);
    },
});

app.MapGet("/", () => Results.Ok(new
{
    name = "TechVerse X API",
    status = "kerangka",
    catatan = "Fase 1 skeleton. Fase 0 belum terkunci — lihat Issues repo.",
}));

app.MapTechnologyEndpoints();

await app.RunAsync().ConfigureAwait(false);

/// <summary>Dibuka supaya proyek uji integrasi bisa memakai WebApplicationFactory.</summary>
public partial class Program;
