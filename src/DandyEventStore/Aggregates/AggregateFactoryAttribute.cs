namespace DandyEventStore.Aggregates;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor)]
public sealed class AggregateFactoryAttribute : Attribute;