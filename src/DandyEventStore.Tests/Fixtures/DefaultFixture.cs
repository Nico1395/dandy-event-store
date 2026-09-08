using DandyEventStore.Configuration;
using DandyEventStore.Persistence.Sql.Npgsql;
using DandyEventStore.Serialization.SystemTextJson;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace DandyEventStore.Tests.Fixtures;

public sealed class DefaultFixture : IServiceProvider, IAsyncLifetime
{
    private readonly ServiceProvider _serviceProvider;
    private readonly PostgreSqlContainer _npgSqlContainer = new PostgreSqlBuilder("postgres:latest")
        .WithDatabase("tests")
        .WithUsername("dev")
        .WithPassword("dev")
        .WithCleanUp(true)
        .Build();

    public DefaultFixture()
    {
        var services = new ServiceCollection();

        services.AddSingleton(_npgSqlContainer);
        services.AddDandyEventStore(cfg =>
        {
            cfg.ScanInAssemblies(typeof(DefaultFixture).Assembly);
            cfg.UseSystemTextJson();
            cfg.UseNpgsql(npgsql =>
            {
                npgsql.WithConnectionString(_npgSqlContainer.GetConnectionString());
            });
        });

        _serviceProvider = services.BuildServiceProvider();
    }

    public object? GetService(Type serviceType)
    {
        return _serviceProvider.GetService(serviceType);
    }

    public async Task InitializeAsync()
    {
        await _npgSqlContainer.StartAsync();

        var migrationRunner = _serviceProvider.GetRequiredService<IMigrationRunner>();
        migrationRunner.MigrateUp();
    }

    public Task DisposeAsync()
    {
        return _npgSqlContainer.StopAsync();
    }
}