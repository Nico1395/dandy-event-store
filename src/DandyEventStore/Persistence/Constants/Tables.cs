namespace DandyEventStore.Persistence.Constants;

public static class Tables
{
    public static class Envelopes
    {
        public const string Table = "envelopes";

        public const string StreamId = "stream_id";
        public const string Payload = "payload";
        public const string Version = "stream_id";
        public const string Timestamp = "timestamp";
        public const string EventType = "event_type";
    }

    public static class Snapshots
    {
        public const string Table = "snapshots";

        public const string StreamId = "stream_id";
        public const string Payload = "payload";
        public const string Version = "stream_id";
        public const string Timestamp = "timestamp";
        public const string AggregateType = "aggregate_type";
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