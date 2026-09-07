namespace DandyEventStore.Projections;

public interface IAsyncProjection<in TEvent> : IProjection<TEvent>
    where TEvent : class
{
}