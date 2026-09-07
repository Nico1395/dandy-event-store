namespace DandyEventStore.Persistence.Sql;

public abstract class SqlStrings
{
    public abstract string GetStreamVersion { get; }
    // TODO: Implement filtering
    public abstract string GetStream  { get; }
    public abstract string StoreEnvelope  { get; }
    public abstract string GetLastSnapshot { get; }
    public abstract string StoreSnapshot { get; }
}