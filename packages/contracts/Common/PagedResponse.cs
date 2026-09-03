namespace TechVerseX.Contracts.Common;

/// <summary>
/// Amplop baku untuk seluruh daftar berhalaman.
/// Rujukan: KERANGKA.md 4.9 — semua API punya bentuk yang seragam.
/// </summary>
public sealed record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalItems)
{
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalItems / (double)PageSize);

    public bool HasNextPage => Page < TotalPages;
}
