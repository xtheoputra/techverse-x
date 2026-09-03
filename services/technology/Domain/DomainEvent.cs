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
