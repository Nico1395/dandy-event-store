namespace DandyEventStore.Persistence.Migrations;

public interface IMigrationService
{
    Task UpAsync(CancellationToken cancellationToken);
    Task DownAsync(long version, CancellationToken cancellationToken);
}