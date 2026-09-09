namespace DandyEventStore.Persistence.Sql.Constants;

public static class Tables
{
    public static class Envelopes
    {
        public const string Table = "envelopes";

        public const string StreamId = "stream_id";
        public const string Payload = "payload";
        public const string Version = "version";
        public const string Timestamp = "timestamp";
        public const string EventKey = "event_key";
    }

    public static class Snapshots
    {
        public const string Table = "snapshots";

        public const string StreamId = "stream_id";
        public const string Payload = "payload";
        public const string Version = "version";
        public const string Timestamp = "timestamp";
        public const string AggregateKey = "aggregate_key";
    }

    public static class OutboxEnvelopes
    {
        public const string Table = "outbox_envelopes";
        
        public const string StreamId = "stream_id";
        public const string Payload = "payload";
        public const string Version = "version";
        public const string Timestamp = "timestamp";
        public const string EventKey = "event_key";
    }

    public static class OutboxEnvelopeConsumers
    {
        public const string Table = "outbox_envelope_consumers";
        
        public const string StreamId = "stream_id";
        public const string Payload = "payload";
        public const string Version = "version";
        public const string ConsumedAt = "consumed_at";
        public const string FailedAt = "failed_at";
        public const string ConsumerKey = "ConsumerKey";
    }
}