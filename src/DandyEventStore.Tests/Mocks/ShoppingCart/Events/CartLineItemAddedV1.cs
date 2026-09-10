namespace DandyEventStore.Tests.Mocks.ShoppingCart.Events;

[Event]
internal sealed record CartLineItemAddedV1(
    Guid CartId,
    Guid ProductId,
    int Quantity);