namespace DandyEventStore.Subscribers;

public interface ISubscriberExceptionHandler<in TEvent>
    where TEvent : class
{
    Task HandleAsync(TEvent @event, SubscriberContext context, Exception exception, CancellationToken cancellationToken);   
}