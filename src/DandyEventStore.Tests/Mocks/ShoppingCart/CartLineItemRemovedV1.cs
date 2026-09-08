using DandyEventStore.Events;

namespace DandyEventStore.Tests.Mocks.ShoppingCart;

[Event]
internal sealed record CartLineItemRemovedV1(
    Guid CartId,
    Guid ProductId);