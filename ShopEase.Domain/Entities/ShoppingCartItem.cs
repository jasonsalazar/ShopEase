namespace ShopEase.Domain.Entities;

public class ShoppingCartItem
{
    private ShoppingCartItem() { }

    internal ShoppingCartItem(int productId, int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(productId);

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);

        ProductId = productId;
        Quantity = quantity;
    }

    public int Id { get; private set; }

    public int ShoppingCartId { get; private set; }

    public int ProductId { get; private set; }

    public int Quantity { get; private set; }

    public Product? Product { get; private set; }

    internal void ChangeQuantity(int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);

        Quantity = quantity;
    }
}
