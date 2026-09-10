namespace DandyEventStore.Tests.Mocks.ShoppingCart.Events;

[Event]
internal sealed record CartLineItemRemovedV1(
    Guid CartId,
    Guid ProductId);