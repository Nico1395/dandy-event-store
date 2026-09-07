using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace DandyEventStore.Projections;

public class Projector(IServiceProvider serviceProvider) : IProjector
{
    public async Task ProjectAsync(IReadOnlyEventStore? eventStore, Envelope[] envelopes, ProjectionMode mode, CancellationToken cancellationToken)
    {
        eventStore ??= serviceProvider.GetRequiredService<IReadOnlyEventStore>();

        var genericProjectionType = mode == ProjectionMode.Immediate ? typeof(IImmediateProjection<>) : typeof(IAsyncProjection<>);

        foreach (var group in envelopes.GroupBy(e => e.RuntimeType))
        {
            var groupEnvelopes = group.ToArray();
            if (groupEnvelopes.Length == 0)
                continue;

            var projectionType = genericProjectionType.MakeGenericType(group.Key);
            var projections = serviceProvider.GetServices(projectionType).Cast<object>().ToArray();

            await ProjectAsync(eventStore, group.Key, groupEnvelopes, projections, mode, cancellationToken);
        }
    }

    private async Task ProjectAsync(IReadOnlyEventStore eventStore, Type eventType, Envelope[] envelopes, object[] projections, ProjectionMode mode, CancellationToken cancellationToken)
    {
        // TODO:
        // - Filter out projection implementations that dont have any retries left
        // - Orchestrate outbox

        if (envelopes.Length == 0 || projections.Length == 0)
            return;

        var projectAsync = typeof(IProjection<>).GetMethod(nameof(IProjection<>.ProjectAsync)) ?? throw new UnreachableException();
        Type? exceptionHandlerType = null;
        MethodInfo? handleAsync = null;

        for (var i = 0; i < envelopes.Length; i++)
        {
            var envelope = envelopes[i];

            for (var j = 0; j < projections.Length; j++)
            {
                var projection = projections[j];
                var context = new ProjectionContext
                {
                    EventStore = eventStore,
                    Envelope = envelope,
                    Mode = mode,
                    ProjectionIndex = j,
                    ProjectionCount = projections.Length,
                };

                try
                {
                    var projectionResult = projectAsync.Invoke(projection, [envelope.Event, context, cancellationToken]);
                    if (projectionResult is not Task projectionTask)
                        throw new UnreachableException();

                    await projectionTask;
                }
                catch (Exception ex)
                {
                    exceptionHandlerType ??= typeof(IProjectionExceptionHandler<>).MakeGenericType(eventType);
                    var exceptionHandler = serviceProvider.GetService(exceptionHandlerType);
                    if (exceptionHandler == null)
                        continue;

                    handleAsync ??= exceptionHandlerType.GetMethod(nameof(IProjectionExceptionHandler<>.HandleAsync)) ?? throw new UnreachableException();

                    var handleResult = handleAsync.Invoke(exceptionHandler, [envelope.Event, context, ex, cancellationToken]);
                    if (handleResult is not Task handleTask)
                        throw new UnreachableException();

                    await handleTask;
                }
            }
        }
    }
}