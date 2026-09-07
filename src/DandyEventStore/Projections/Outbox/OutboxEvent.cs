namespace DandyEventStore.Projections.Outbox;

internal sealed class OutboxEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string StreamId { get; set; }
    public required long Version { get; set; }
    public required object Event { get; set; }
    public List<OutboxEventProjection> Projections { get; set; } = new();
    public DateTime PublishedAt { get; set; } = DateTime.UtcNow;
}