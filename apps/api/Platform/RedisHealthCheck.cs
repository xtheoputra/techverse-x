using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace TechVerseX.Api.Platform;

/// <summary>
/// Health check Redis yang benar-benar melakukan PING, bukan sekadar
/// memastikan objek koneksinya ada.
/// </summary>
/// <remarks>
/// KERANGKA.md 4.13 menutup bagian Disaster Recovery dengan pertanyaan
/// <em>"apakah backup itu pernah berhasil di-restore?"</em>. Semangat yang sama
/// dipakai di sini: health check yang tidak pernah menyentuh dependensinya
/// hanya melaporkan bahwa proses sendiri masih hidup.
/// </remarks>
public sealed class RedisHealthCheck(IConnectionMultiplexer redis) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var latency = await redis.GetDatabase().PingAsync().ConfigureAwait(false);

            return HealthCheckResult.Healthy(
                $"Redis membalas dalam {latency.TotalMilliseconds:F1} ms.");
        }
        catch (RedisConnectionException ex)
        {
            return HealthCheckResult.Unhealthy("Redis tidak bisa dihubungi.", ex);
        }
        catch (RedisTimeoutException ex)
        {
            return HealthCheckResult.Unhealthy("Redis tidak membalas tepat waktu.", ex);
        }
    }
}
