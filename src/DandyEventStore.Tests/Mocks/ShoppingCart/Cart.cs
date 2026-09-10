using DandyEventStore.Aggregates;
using DandyEventStore.Tests.Mocks.ShoppingCart.Events;

namespace DandyEventStore.Tests.Mocks.ShoppingCart;

[Aggregate(SnapshotInterval = 5)]
internal sealed class Cart
{
    public required Guid Id { get; init; }
    public long Version { get; private set; }
    public required string UserId { get; init; }
    public List<CartLineItem> LineItems { get; init; } = new();
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    [AggregateFactory]
    public static Cart Create(Cart? snapshot, Envelope[] envelopes)
    {
        if (snapshot == null)
        {
            var cartCreated = envelopes
                .Select(e => e.Event)
                .OfType<CartCreatedV1>()
                .SingleOrDefault();

            if (cartCreated == null)
                throw new InvalidOperationException();

            snapshot = new Cart
            {
                Id = cartCreated.CartId,
                UserId = cartCreated.UserId,
            };
        }

        snapshot.Apply(envelopes);
        return snapshot;
    }

    public void Apply(Envelope[] envelopes)
    {
        foreach (var envelope in envelopes)
        {
            switch (envelope.Event)
            {
                case CartCreatedV1:
                    continue;
                case CartLineItemAddedV1 lineItemAdded:
                    Apply(lineItemAdded);
                    break;
                case CartLineItemQuantityAddedV1 quantityAdded:
                    Apply(quantityAdded);
                    break;
                case CartLineItemRemovedV1 lineItemRemoved:
                    Apply(lineItemRemoved);
                    break;
                default:
                    throw new InvalidOperationException();
            }

            UpdatedAt = envelope.Timestamp;
            Version = envelope.Version;
        }
    }

    public bool Apply(CartLineItemAddedV1 cartLineItemAddedV1)
    {
        var lineItem = LineItems.SingleOrDefault(l => l.ProductId == cartLineItemAddedV1.ProductId);
        if (lineItem != null)
            return lineItem.AddQuantity(cartLineItemAddedV1.Quantity);

        if (cartLineItemAddedV1.Quantity < 0)
            return false;

        LineItems.Add(new CartLineItem
        {
            CartId = Id,
            ProductId = cartLineItemAddedV1.ProductId,
            Quantity = cartLineItemAddedV1.Quantity,
        });

        return true;
    }

    public bool Apply(CartLineItemQuantityAddedV1 cartLineItemQuantityAddedV1)
    {
        var lineItem = LineItems.SingleOrDefault(l => l.ProductId == cartLineItemQuantityAddedV1.ProductId);
        return lineItem != null && lineItem.AddQuantity(cartLineItemQuantityAddedV1.Quantity);
    }

    public bool Apply(CartLineItemRemovedV1 cartLineItemRemovedV1)
    {
        return LineItems.RemoveAll(l => l.ProductId == cartLineItemRemovedV1.ProductId) == 1;
    }
}