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

        await eventStore.AppendAsync(cartCreated.CartId.ToString(), [cartCreated], cancellationToken: CancellationToken.None);

        var stream = await eventStore.GetStreamAsync(cartCreated.CartId.ToString(), null, null, null, null, cancellationToken: CancellationToken.None);
        Assert.Single(stream);

        var aggregate = await eventStore.ReplayAggregateAsync<Cart>(cartCreated.CartId.ToString(), null, null, CancellationToken.None);
        Assert.NotNull(aggregate);
    }
}