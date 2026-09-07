namespace DandyEventStore.Projections;

public interface IImmediateProjection<in TEvent> : IProjection<TEvent>
    where TEvent : class
{
}