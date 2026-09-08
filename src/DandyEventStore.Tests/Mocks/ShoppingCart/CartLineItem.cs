namespace DandyEventStore.Tests.Mocks.ShoppingCart;

internal sealed class CartLineItem
{
    public Guid CartId { get; init; }
    public Guid ProductId { get; init; }
    public int Quantity { get; set; }

    public bool AddQuantity(int quantity)
    {
        var newQuantity = Quantity + quantity;
        if (newQuantity < 0)
            return false;

        Quantity = newQuantity;
        return true;
    }
}