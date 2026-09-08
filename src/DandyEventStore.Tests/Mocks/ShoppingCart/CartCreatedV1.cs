using DandyEventStore.Events;

namespace DandyEventStore.Tests.Mocks.ShoppingCart;

[Event]
internal sealed record CartCreatedV1(
    Guid CartId,
    string UserId);