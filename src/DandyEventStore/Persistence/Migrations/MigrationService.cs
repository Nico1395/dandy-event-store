using FluentMigrator.Runner;

namespace DandyEventStore.Persistence.Migrations;

internal sealed class MigrationService(IMigrationRunner migrationRunner) : IMigrationService
{
    public async Task UpAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!migrationRunner.HasMigrationsToApplyUp())
            return;

        migrationRunner.MigrateUp();
        await Task.CompletedTask;
    }

    public async Task DownAsync(long version, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!migrationRunner.HasMigrationsToApplyDown(version))
            return;

        migrationRunner.MigrateDown(version);
        await Task.CompletedTask;
    }
}