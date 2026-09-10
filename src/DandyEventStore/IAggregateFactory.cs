namespace DandyEventStore;

public interface IAggregateFactory<TAggregate>
    where TAggregate : class
{
    TAggregate Create(TAggregate? snapshot, Envelope[] envelopes);
}