namespace DandyEventStore.Persistence.Configuration;

public sealed class PersistenceConfigurationBuilder
{
    public string? ConnectionString { get; set; }
    public Type? SqlStringsType { get; set; }
    public Type? DbConnectionFactoryType { get; set; }

    internal PersistenceConfiguration Build()
    {
        return new PersistenceConfiguration
        {
            ConnectionString = ConnectionString,
            SqlStringsType = SqlStringsType,
            ConnectionFactoryType = DbConnectionFactoryType
        };
    }
}