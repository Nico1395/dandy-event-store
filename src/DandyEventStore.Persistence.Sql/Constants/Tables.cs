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

    public static class Outbox
    {
        public const string Table = "outbox";
    }

    public static class OutboxProjections
    {
        public const string Table = "outbox_projections";
    }
}