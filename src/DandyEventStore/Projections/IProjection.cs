namespace DandyEventStore.Projections;

public interface IProjection<in TEvent>
    where TEvent : class
{
    Task ProjectAsync(TEvent @event, ProjectionContext context, CancellationToken cancellationToken);
}