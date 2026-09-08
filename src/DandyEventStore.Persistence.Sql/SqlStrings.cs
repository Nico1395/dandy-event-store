namespace DandyEventStore.Persistence.Sql;

public abstract class SqlStrings
{
    public abstract string GetStreamVersion { get; }
    public abstract string GetStream  { get; }      // TODO: Implement filtering
    public abstract string StoreEnvelope  { get; }
    public abstract string GetLastSnapshot { get; }
    public abstract string StoreSnapshot { get; }
}