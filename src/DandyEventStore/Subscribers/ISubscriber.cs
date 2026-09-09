namespace DandyEventStore.Subscribers;

public interface ISubscriber<in TEvent>
    where TEvent : class
{
    Task HandleAsync(TEvent @event, SubscriberContext context, CancellationToken cancellationToken);
}