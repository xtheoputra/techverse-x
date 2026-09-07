namespace TechVerseX.Contracts.Technology;

/// <summary>
/// Satu bidang teknologi. Empat belas di antaranya (ADR-010).
/// </summary>
/// <remarks>
/// <c>ReviewedTopicCount</c> dibawa bersama <c>TopicCount</c> dengan sengaja: satu
/// angka menyatakan seberapa banyak yang ADA, satu lagi seberapa banyak yang sudah
/// DIPERIKSA MANUSIA. Angka pertama bisa dinaikkan mesin dalam semenit; angka
/// kedua tidak bisa, dan itulah ukuran kemajuan menurut docs/RENCANA-V1.md.
/// </remarks>
public sealed record FieldResponse(
    Guid Id,
    string Slug,
    string Name,
    string Summary,
    string Priority,
    int DisplayOrder,
    int TopicCount,
    int ReviewedTopicCount);
