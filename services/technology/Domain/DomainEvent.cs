namespace TechVerseX.TechnologyService.Domain;

/// <summary>
/// Amplop event dari KERANGKA.md 4.8 — <b>belum utuh</b>: <c>correlationId</c> belum ada.
/// Bus-nya sendiri (NATS di dev, Kafka di prod) BELUM dipasang — lihat ADR-004.
/// Untuk sementara event dikumpulkan di agregat, lalu dibuang <c>ClearEvents()</c>
/// sesudah disimpan.
/// </summary>
/// <remarks>
/// ⚠️ <see cref="EventId"/> dan <see cref="OccurredAt"/> tidak punya pembaca di kode
/// produksi, dan itu keputusan ADR-004 (bentuk sekarang, pengangkutan nanti), bukan
/// kode mati. Yang perlu diingat saat bus dipasang: amplop ini masih kurang
/// <c>correlationId</c>, jadi menambahkannya <b>adalah</b> perubahan kontrak —
/// lihat pembaruan 2026-09-28 di ADR-004.
/// <para>
/// Sampai 2026-09-28 ringkasan ini menunjuk "ADR-0005" — nomor yang tidak ada;
/// ADR-005 membahas Qdrant — dan menulis bahwa event "dibaca saat SaveChanges".
/// Tidak ada yang membacanya di sana: tidak ada interceptor, tidak ada override,
/// dan <c>TechnologyConfiguration</c> meng-<c>Ignore</c> daftarnya.
/// </para>
/// </remarks>
public abstract record DomainEvent
{
    public Guid EventId { get; } = Guid.CreateVersion7();

    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;

    public abstract string EventType { get; }

    /// <summary>Versi skema event. Naikkan kalau bentuk muatannya berubah.</summary>
    public int Version { get; init; } = 1;
}

public sealed record TechnologyCreated(Guid TechnologyId, string Slug, string Name) : DomainEvent
{
    public override string EventType => nameof(TechnologyCreated);
}

public sealed record TechnologyUpdated(Guid TechnologyId, string Slug) : DomainEvent
{
    public override string EventType => nameof(TechnologyUpdated);
}

public sealed record TechnologyPublished(Guid TechnologyId, string Slug) : DomainEvent
{
    public override string EventType => nameof(TechnologyPublished);
}

/// <summary>Isi halaman naik ke tingkat "sudah diperiksa manusia" (ADR-012).</summary>
public sealed record TechnologyReviewed(Guid TechnologyId, string Slug, string Reviewer) : DomainEvent
{
    public override string EventType => nameof(TechnologyReviewed);
}

/// <summary>
/// Isi halaman berubah setelah diperiksa, jadi pemeriksaannya gugur.
/// Event tersendiri, bukan sekadar bagian dari TechnologyUpdated: yang satu
/// berarti "ada yang berubah", yang ini berarti "yang tadinya tepercaya sudah
/// tidak lagi" - dan pembaca hilirnya berbeda.
/// </summary>
public sealed record TechnologyReviewExpired(Guid TechnologyId, string Slug) : DomainEvent
{
    public override string EventType => nameof(TechnologyReviewExpired);
}
