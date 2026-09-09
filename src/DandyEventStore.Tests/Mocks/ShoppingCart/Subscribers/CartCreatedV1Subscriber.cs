using DandyEventStore.Subscribers;
using DandyEventStore.Tests.Mocks.ShoppingCart.Events;

namespace DandyEventStore.Tests.Mocks.ShoppingCart.Subscribers;

internal sealed class CartCreatedV1Subscriber : ISubscriber<CartCreatedV1>
{
    public Task HandleAsync(CartCreatedV1 @event, SubscriberContext context, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}