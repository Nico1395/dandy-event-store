using System.Diagnostics;
using DandyEventStore.Configuration;
using DandyEventStore.Persistence;
using DandyEventStore.Persistence.Mapping;
using DandyEventStore.Serialization;
using DandyEventStore.Subscribers;
using Microsoft.Extensions.Hosting;

namespace DandyEventStore.Outbox;

internal sealed class AsyncOutboxDaemon(
    EventStoreConfiguration eventStoreConfiguration,
    IServiceProvider serviceProvider,
    ISubscriptionManager subscriptionManager,
    ISerializer serializer,
    IUnitOfWork unitOfWork) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                await IterateAsync(stoppingToken);
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

    private async Task IterateAsync(CancellationToken cancellationToken)
    {
        var rawEnvelopes = await unitOfWork.Outbox.GetEnvelopesAsync(cancellationToken);

        // Remove expired envelopes first, we don't want to consume those again
        var expired = rawEnvelopes.Where(e =>
        {
            if (!eventStoreConfiguration.Events.EventConfigsByKey.TryGetValue(e.EventKey, out var configuration))
                throw new InvalidOperationException($"Envelope type {e.EventKey} is not configured.");

            var lifeTime = configuration.Lifetime ?? eventStoreConfiguration.Events.DefaultLifetime;
            return DateTime.UtcNow >= e.Timestamp + lifeTime;
        }).ToArray();

        if (expired.Length > 0)
        {
            await unitOfWork.Outbox.DeleteEnvelopesAsync(expired, cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);
        }

        var envelopes = InternalMapper.MapFromEntity(eventStoreConfiguration, serializer, rawEnvelopes.Except(expired)).ToArray();
        foreach (var envelope in envelopes)
        {
            // The subscription manager filters out subscribers that have already successfully consumed the envelope
            // using the envelope's consumers and their statuses.

            await subscriptionManager.NotifySubscribersAsync(
                eventStore: null,
                envelope,
                [SubscriberMode.Inline, SubscriberMode.Async],
                cancellationToken);
        }

        var consumers = envelopes.SelectMany(e => e.Consumers).ToArray();

        var consumersToInsert = consumers.Where(c => c.IsNew);
        var insertEntities = InternalMapper.MapToEntity(consumersToInsert).ToArray();
        await unitOfWork.Outbox.InsertConsumersAsync(insertEntities, cancellationToken);

        var consumersToUpdate = consumers.Where(c => !c.IsNew);
        var updateEntities = InternalMapper.MapToEntity(consumersToUpdate).ToArray();
        await unitOfWork.Outbox.UpdateConsumersAsync(updateEntities, cancellationToken);

        await unitOfWork.CommitAsync(cancellationToken);
    }
}