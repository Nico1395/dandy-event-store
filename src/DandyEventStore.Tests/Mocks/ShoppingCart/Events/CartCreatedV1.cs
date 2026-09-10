namespace DandyEventStore.Tests.Mocks.ShoppingCart.Events;

[Event]
internal sealed record CartCreatedV1(
    Guid CartId,
    string UserId);