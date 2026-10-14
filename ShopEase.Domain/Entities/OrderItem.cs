
namespace ShopEase.Domain.Entities;

public class OrderItem
{
    private OrderItem() { }

    internal OrderItem(
        int productId,
        string productName,
        decimal unitPrice,
        int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(productId);

        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException(
                "Product name is required.", nameof(productName));

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(unitPrice);

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);

        ProductId = productId;
        ProductName = productName;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }

    public int Id { get; private set; }
    public int OrderId { get; private set; }
    public int ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }

    public decimal Subtotal => UnitPrice * Quantity;
}
