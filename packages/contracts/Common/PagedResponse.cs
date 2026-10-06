namespace TechVerseX.Contracts.Common;

/// <summary>
/// Amplop baku untuk seluruh daftar berhalaman.
/// </summary>
/// <remarks>
/// <see cref="TotalPages"/> dan <see cref="HasNextPage"/> bukan hiasan: endpoint
/// daftar menerima <c>?page=</c>, dan klien yang benar-benar menomori halaman
/// membutuhkan keduanya. Web sendiri <b>tidak</b> menomori halaman — ia menampilkan
/// N teratas dan menandai potongannya dengan <c>totalItems &gt; items.length</c> —
/// jadi sapuan pemanggil-nol keempat (#68) mencatat keduanya tanpa pembaca. Sejak
/// 2026-09-28 pembaca pertamanya di repo ini pemindai halaman
/// (<c>.github/scripts/periksa-halaman-web.mjs</c>), yang sebelumnya diam-diam
/// berhenti di 20 topik karena hanya membaca halaman pertama.
/// <para>
/// Sampai 2026-09-28 ringkasan ini merujuk KERANGKA.md 4.9 untuk kalimat "semua
/// API punya bentuk yang seragam". Kalimat itu tidak ada di sana — 4.9 hanya
/// mendaftar rute dan syarat lintas-API (OpenAPI, versioning, dan seterusnya).
/// </para>
/// </remarks>
public sealed record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalItems)
{
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalItems / (double)PageSize);

    public bool HasNextPage => Page < TotalPages;
}
