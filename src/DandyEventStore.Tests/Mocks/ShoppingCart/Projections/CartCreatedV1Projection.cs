using DandyEventStore.Projections;
using DandyEventStore.Tests.Mocks.ShoppingCart.Events;

namespace DandyEventStore.Tests.Mocks.ShoppingCart.Projections;

internal sealed class CartCreatedV1Projection : IImmediateProjection<CartCreatedV1>
{
    public Task ProjectAsync(CartCreatedV1 @event, ProjectionContext context, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}