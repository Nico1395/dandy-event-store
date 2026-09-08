namespace DandyEventStore.Persistence.Sql.Npgsql;

public sealed class NpgsqlConfigurationBuilder
{
    private readonly NpgsqlConfiguration _configuration = new();

    public NpgsqlConfigurationBuilder WithConnectionString(string connectionString)
    {
        _configuration.ConnectionString = connectionString;
        return this;
    }

    internal NpgsqlConfiguration Build()
    {
        return _configuration;   
    }
}