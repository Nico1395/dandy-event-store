namespace DandyEventStore.Persistence.Configuration;

public sealed class PersistenceConfiguration
{
    public string? ConnectionString { get; init; }
    public Type? SqlStringsType { get; init; }
    public Type? ConnectionFactoryType { get; init; }
}