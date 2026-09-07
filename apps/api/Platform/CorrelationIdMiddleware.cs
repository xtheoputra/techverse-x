namespace TechVerseX.Api.Platform;

/// <summary>
/// Memberi setiap request satu correlation id dan menempelkannya ke scope log.
/// </summary>
/// <remarks>
/// KERANGKA.md 4.9 mewajibkan Correlation ID pada semua API, dan 4.1 P6
/// ("Everything Observable") menuntut tiap request penting bisa ditelusuri.
/// Ini potongan paling murah dari janji itu, dan yang paling mahal kalau
/// baru dipasang setelah ada insiden.
/// </remarks>
public sealed class CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
{
    public const string HeaderName = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var correlationId = context.Request.Headers.TryGetValue(HeaderName, out var incoming)
                            && !string.IsNullOrWhiteSpace(incoming)
            ? incoming.ToString()
            : Guid.CreateVersion7().ToString();

        context.TraceIdentifier = correlationId;
        context.Response.Headers[HeaderName] = correlationId;

        using (logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
        {
            await next(context).ConfigureAwait(false);
        }
    }
}
