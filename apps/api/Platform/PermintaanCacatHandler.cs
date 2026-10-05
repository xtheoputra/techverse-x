using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;

namespace TechVerseX.Api.Platform;

/// <summary>
/// Permintaan yang tidak bisa diikat ke parameter endpoint dijawab dengan status yang
/// dibawanya sendiri — 400 untuk JSON yang terpotong, kosong, atau bertipe salah —
/// berikut sebabnya, bukan 500 (issue #61).
/// </summary>
/// <remarks>
/// 🔴 <b>ASP.NET punya dua jalan untuk permintaan yang sama, dan tak satu pun benar
/// di repo ini sebelum kelas ini ada.</b> Diukur 2026-09-17 dengan muatan yang sama ke
/// kedelapan endpoint tulis berbadan dan ke <c>?pageSize=abc</c>:
/// <list type="bullet">
/// <item><c>Development</c> — <c>RouteHandlerOptions.ThrowOnBadRequest</c> bawaannya
/// hidup, jadi pengikatan MELEMPAR <c>BadHttpRequestException</c> (yang membawa status
/// 400), dan <c>UseExceptionHandler()</c> menjawab <b>500</b> tanpa membaca status itu.
/// Log mencatatnya sebagai <c>fail</c>: galat server untuk kesalahan pemanggil.</item>
/// <item>Lingkungan lain — sakelar itu mati, jadi pengikatan hanya menulis <b>400
/// tanpa satu kata pun tentang sebabnya</b>. Sebabnya tinggal di log tingkat
/// <c>Debug</c>, yang tidak pernah tercetak.</item>
/// </list>
/// Semua uji integrasi berjalan di <c>Development</c>, jadi jalan kedua — yang dipakai
/// produksi — tidak pernah dilihat satu uji pun.
/// <para>
/// 🔑 <b>Karena itu <c>Program.cs</c> menyalakan <c>ThrowOnBadRequest</c> di SEMUA
/// lingkungan</b>, dan kelas ini jadi satu-satunya jalan: uji yang berjalan di
/// <c>Development</c> kini menguji jalan yang sama dengan produksi.
/// </para>
/// <para>
/// <c>detail</c> berisi pesan kerangka kerjanya apa adanya — yang menyebut parameter
/// dan, untuk JSON, jalurnya (<c>Path: $.type</c>). Sengaja tidak diterjemahkan: pesan
/// itu yang paling tepat, dan yang ditulis ulang tangan akan tertinggal saat
/// kerangka kerjanya berubah. Hanya pesan <see cref="JsonException"/> yang ikut
/// disertakan dari exception di dalamnya; exception dalam jenis lain tidak pernah
/// dibocorkan.
/// </para>
/// <para>
/// ⚠️ Diagnostik <c>ExceptionHandlerMiddleware</c> — baris <c>fail</c> "An unhandled
/// exception has occurred" — tidak dicatat untuk exception yang ditangani
/// <see cref="IExceptionHandler"/> (bawaan .NET 10). Yang tersisa di log adalah baris
/// permintaan biasa berstatus 400, dan sebabnya ada di jawaban.
/// </para>
/// </remarks>
public sealed class PermintaanCacatHandler(IProblemDetailsService problemDetails) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        if (exception is not BadHttpRequestException cacat)
        {
            return false;
        }

        httpContext.Response.StatusCode = cacat.StatusCode;

        await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails =
            {
                Status = cacat.StatusCode,
                Detail = cacat.InnerException is JsonException json
                    ? $"{cacat.Message} {json.Message}"
                    : cacat.Message,
            },
        }).ConfigureAwait(false);

        // Tetap TERTANGANI walau badannya tidak bisa ditulis - misalnya pemanggil yang
        // hanya menerima text/html: 400 tanpa badan masih jawaban yang benar.
        // Terukur 2026-09-17: mengembalikan hasil TryWriteAsync (false) di sini tidak
        // mengubah statusnya, tapi membuat kesalahan PEMANGGIL tercatat sebagai `fail`
        // "An unhandled exception has occurred" - galat server palsu di log.
        return true;
    }
}
