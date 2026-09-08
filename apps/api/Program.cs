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

// Redis OPSIONAL, tidak seperti Postgres.
//
// ADR-016 memutuskan Redis TIDAK di-provision di produksi V1 — ia harus
// membuktikan dirinya dulu dengan beban yang benar-benar ada. Selama kesiapan
// menuntut Redis, penyebaran tidak pernah selesai: platform peti kemas memakai
// /health/ready untuk memutuskan apakah boleh mengalirkan lalu lintas, dan
// gejalanya menyamar jadi "aplikasinya lambat menyala".
//
// Tidak dikonfigurasi = tidak dipasang. Bukan "dipasang lalu dilaporkan sehat".
var redisConnection = builder.Configuration.GetConnectionString("Redis");
var redisTerpasang = !string.IsNullOrWhiteSpace(redisConnection);

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

if (redisTerpasang)
{
    builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    {
        var options = ConfigurationOptions.Parse(redisConnection!);

        // 🔴 Tanpa baris ini, Connect() MELEMPAR di dalam pabrik DI — yaitu saat
        // RedisHealthCheck sedang dibangun, sebelum CheckHealthAsync sempat
        // berjalan. Akibatnya try/catch di dalam health check itu tidak pernah
        // dijalankan untuk kasus yang justru ia tulis, dan /health/ready membalas
        // 500 (galat server) alih-alih 503 dengan nama dependensi yang jatuh.
        options.AbortOnConnectFail = false;

        return ConnectionMultiplexer.Connect(options);
    });
}

builder.Services.AddTechnologyService(postgres);

// ---- Health checks (T010) --------------------------------------------------
// Dipisah dua tag supaya Kubernetes bisa membedakan "proses hidup" dari
// "siap menerima lalu lintas" — lihat KERANGKA.md 4.12.
var healthChecks = builder.Services.AddHealthChecks()
    .AddDbContextCheck<TechnologyDbContext>(
        name: "postgres",
        failureStatus: HealthStatus.Unhealthy,
        tags: ["ready"]);

if (redisTerpasang)
{
    healthChecks.AddCheck<RedisHealthCheck>(
        name: "redis",
        failureStatus: HealthStatus.Unhealthy,
        tags: ["ready"]);
}

var app = builder.Build();

if (!redisTerpasang)
{
    CatatanStartup.RedisTidakTerpasang(app.Logger);
}

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

/// <summary>
/// Pesan startup sebagai delegasi <c>LoggerMessage</c> yang dibangkitkan sumber.
/// </summary>
/// <remarks>
/// Bukan gaya, melainkan tuntutan analyzer CA1848 yang menyala karena repo ini
/// memperlakukan peringatan sebagai galat. Memanggil <c>LogInformation</c>
/// langsung akan menggagalkan build.
/// </remarks>
internal static partial class CatatanStartup
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Redis tidak dikonfigurasi - health check 'redis' tidak dipasang. Ini bentuk produksi V1 menurut ADR-016.")]
    public static partial void RedisTidakTerpasang(ILogger logger);
}
