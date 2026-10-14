namespace ShopEase.Domain.Entities;

public class ShoppingCart
{
    private readonly List<ShoppingCartItem> _items = [];

    private ShoppingCart() { }

    public ShoppingCart(string cartKey)
    {
        if (string.IsNullOrWhiteSpace(cartKey))
            throw new ArgumentException(
                "Cart key is required.", nameof(cartKey));

        Id = 0;
        CartKey = cartKey;
    }

    public int Id { get; private set; }

    public string CartKey { get; private set; } = string.Empty;

    public IReadOnlyCollection<ShoppingCartItem> Items => _items;

    public void AddItem(int productId, int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(productId);

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);

        var existing = _items.FirstOrDefault(
            item => item.ProductId == productId);

        if (existing is null)
        {
            _items.Add(new ShoppingCartItem(productId, quantity));
        }
        else
        {
            existing.ChangeQuantity(
                checked(existing.Quantity + quantity));
        }
    }

    public void UpdateItem(int productId, int quantity)
    {
        var item = _items.FirstOrDefault(
            entry => entry.ProductId == productId);

        if (item is null)
            throw new InvalidOperationException(
                "The product is not in this cart.");

        if (quantity <= 0)
        {
            _items.Remove(item);
            return;
        }

        item.ChangeQuantity(quantity);
    }

    public void RemoveItem(int productId)
    {
        var item = _items.FirstOrDefault(
            entry => entry.ProductId == productId);

        if (item is not null)
            _items.Remove(item);
    }
}
