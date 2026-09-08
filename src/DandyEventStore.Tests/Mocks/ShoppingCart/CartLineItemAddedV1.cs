using DandyEventStore.Events;

namespace DandyEventStore.Tests.Mocks.ShoppingCart;

[Event]
internal sealed record CartLineItemAddedV1(
    Guid CartId,
    Guid ProductId,
    int Quantity);