using System.Diagnostics;
using DandyEventStore.Configuration;
using Microsoft.Extensions.Hosting;

namespace DandyEventStore.Outbox;

internal sealed class AsyncOutboxDaemon(
    EventStoreConfiguration eventStoreConfiguration,
    IServiceProvider serviceProvider,
    IOutbox outbox) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                await outbox.CheckAndProcessAsync(stoppingToken);
            }
            catch (Exception exception)
            {
                eventStoreConfiguration.OnDaemonIterationException?.Invoke(serviceProvider, exception);
            }

            stopwatch.Stop();
            var remainingTime = eventStoreConfiguration.Outbox.Interval - stopwatch.Elapsed;

            await Task.Delay(remainingTime, stoppingToken);
        }
    }
}