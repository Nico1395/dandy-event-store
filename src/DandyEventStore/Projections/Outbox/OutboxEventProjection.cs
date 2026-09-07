namespace DandyEventStore.Projections.Outbox;

internal sealed class OutboxEventProjection
{
    public required Guid EventId { get; set; }
    public required string ProjectionName { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? FailedAt { get; set; }
}