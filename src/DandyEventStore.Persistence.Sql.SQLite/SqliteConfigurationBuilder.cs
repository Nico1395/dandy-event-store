namespace DandyEventStore.Persistence.Sql.SQLite;

public sealed class SqliteConfigurationBuilder
{
    private readonly SqliteConfiguration _configuration = new();

    public SqliteConfigurationBuilder WithConnectionString(string connectionString)
    {
        _configuration.ConnectionString = connectionString;
        return this;
    }

    internal SqliteConfiguration Build()
    {
        return _configuration;   
    }
}