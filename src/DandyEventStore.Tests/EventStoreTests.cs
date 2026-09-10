using DandyEventStore.Tests.Fixtures;
using DandyEventStore.Tests.Mocks.ShoppingCart;
using DandyEventStore.Tests.Mocks.ShoppingCart.Events;
using Microsoft.Extensions.DependencyInjection;

namespace DandyEventStore.Tests;

public class EventStoreTests(DefaultFixture fixture) : IClassFixture<DefaultFixture>
{
    [Fact]
    public async Task AppendEvents_ShouldAppendEventsToStream()
    {
        var eventStore = fixture.GetRequiredService<IEventStore>();
        var cartCreated = new CartCreatedV1(Guid.NewGuid(), "123");
        var streamId = cartCreated.CartId.ToString();

        await eventStore.AppendAsync<Cart>(
            streamId,
            cartCreated,
            cancellationToken: CancellationToken.None);

        var stream = await eventStore.GetStreamAsync(streamId, null, null, null, null, cancellationToken: CancellationToken.None);
        Assert.Single(stream);

        var aggregate = await eventStore.ReplayAggregateAsync<Cart>(streamId, null, null, CancellationToken.None);
        Assert.NotNull(aggregate);
    }

    [Fact]
    public async Task AppendEvents_ShouldAppendEventsToStream_ShouldCreateSnapshot()
    {
        var eventStore = fixture.GetRequiredService<IEventStore>();

        var cartCreated = new CartCreatedV1(Guid.NewGuid(), "456");
        var streamId = cartCreated.CartId.ToString();
        var productId = Guid.NewGuid();
        var productAAdded = new CartLineItemAddedV1(cartCreated.CartId, productId, 10);
        var productAQuantityAdded = new CartLineItemQuantityAddedV1(cartCreated.CartId, productId, 4);
        var productBAdded = new CartLineItemAddedV1(cartCreated.CartId, Guid.NewGuid(), 3);
        var productCAdded = new CartLineItemAddedV1(cartCreated.CartId, Guid.NewGuid(), 10);
        var productDAdded = new CartLineItemAddedV1(cartCreated.CartId, Guid.NewGuid(), 5);

        await eventStore.AppendAsync<Cart>(
            streamId,
            [cartCreated, productAAdded, productAQuantityAdded, productBAdded, productCAdded, productDAdded],
            cancellationToken: CancellationToken.None);

        var stream = await eventStore.GetStreamAsync(streamId, null, null, null, null, cancellationToken: CancellationToken.None);
        Assert.Equal(6, stream.Length);

        var aggregate = await eventStore.ReplayAggregateAsync<Cart>(streamId, null, null, CancellationToken.None);
        Assert.NotNull(aggregate);
        Assert.Equal(5, aggregate.Version);
    }
}