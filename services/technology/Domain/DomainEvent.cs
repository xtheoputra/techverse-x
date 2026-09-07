namespace TechVerseX.TechnologyService.Domain;

/// <summary>
/// Amplop event seperti didefinisikan di KERANGKA.md 4.8.
/// Bus-nya sendiri (NATS di dev, Kafka di prod) BELUM dipasang — lihat ADR-0005.
/// Untuk sementara event dikumpulkan di aggregate dan dibaca saat SaveChanges.
/// </summary>
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
