namespace DandyEventStore.Projections;

public interface IProjectionExceptionHandler<in TEvent>
    where TEvent : class
{
    Task HandleAsync(TEvent @event, ProjectionContext context, Exception exception, CancellationToken cancellationToken);   
}