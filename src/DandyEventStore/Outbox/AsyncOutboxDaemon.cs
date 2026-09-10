using System.Diagnostics;
using DandyEventStore.Configuration;
using DandyEventStore.Persistence;
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

        var envelopes = GetEnvelopes(rawEnvelopes.Except(expired));
        foreach (var envelope in envelopes)
        {
            // The subscription manager filters out subscribers that have already successfully consumed the envelope
            // using the envelope's consumers and their statuses.

            await subscriptionManager.NotifySubscribersAsync(
                envelope,
                [SubscriberMode.Inline, SubscriberMode.Async],
                cancellationToken);
        }

        var consumers = envelopes.SelectMany(e => e.Consumers);
        await unitOfWork.Outbox.InsertConsumersAsync(GetRaw(consumers).ToArray(), cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
    }

    private OutboxEnvelope[] GetEnvelopes(IEnumerable<RawOutboxEnvelope> rawEnvelopes)
    {
        var envelopes = rawEnvelopes.Select(r =>
        {
            if (!eventStoreConfiguration.Events.EventConfigsByKey.TryGetValue(r.EventKey, out var configuration))
                throw new InvalidOperationException($"Envelope type {r.EventKey} is not configured.");

            var deserialized = serializer.Deserialize(r.Payload, configuration.RuntimeType);
            if (deserialized == null)
                throw new InvalidOperationException($"Failed to deserialize event {r.EventKey} from payload.");

            return new OutboxEnvelope
            {
                StreamId = r.StreamId,
                Event = deserialized,
                Version = r.Version,
                Timestamp = r.Timestamp,
                EventKey = r.EventKey,
                RuntimeType = configuration.RuntimeType,
                Consumers = r.Consumers.Select(c => new OutboxEnvelopeConsumer
                {
                    StreamId = c.StreamId,
                    Version = c.Version,
                    ConsumerKey = c.ConsumerKey,
                    Type = c.Type,
                    ConsumedAt = c.ConsumedAt,
                    FailedAt = c.FailedAt,
                }).ToList(),
            };
        });

        return envelopes.ToArray();
    }

    private IEnumerable<RawOutboxEnvelopeConsumer> GetRaw(IEnumerable<OutboxEnvelopeConsumer> consumers)
    {
        return consumers.Select(c => new RawOutboxEnvelopeConsumer
        {
            StreamId = c.StreamId,
            Version = c.Version,
            ConsumerKey = c.ConsumerKey,
            Type = c.Type,
            ConsumedAt = c.ConsumedAt,
            FailedAt = c.FailedAt,
        });
    }
}