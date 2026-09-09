namespace DandyEventStore.Subscribers;

public interface IAsyncSubscriber<in TEvent> : ISubscriber<TEvent>
    where TEvent : class
{
}