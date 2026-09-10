namespace DandyEventStore.Persistence.Sql;

public abstract class SqlStrings
{
    public abstract string GetStreamVersion { get; }
    public abstract string GetStream  { get; }
    public abstract string InsertEnvelope  { get; }
    public abstract string GetLastSnapshot { get; }
    public abstract string StoreSnapshot { get; }
    public abstract string GetOutboxEnvelopes { get; }
    public abstract string InsertOutboxEnvelopes { get; }
    public abstract string DeleteOutboxEnvelopes { get; }
    public abstract string InsertOutboxEnvelopeConsumers { get; }
    public abstract string UpdateOutboxEnvelopeConsumers { get; }
}