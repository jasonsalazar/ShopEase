namespace ShopEase.Application.DTOs;

public class ShoppingCartItemDto
{
    public int ProductId { get; init; }

    public string Name { get; init; } = string.Empty;

    public string? ImageUrl { get; init; }

    public string CategoryName { get; init; } = string.Empty;

    public decimal UnitPrice { get; init; }

    public int Quantity { get; init; }

    public int StockQuantity { get; init; }

    public decimal Subtotal => UnitPrice * Quantity;

    public bool IsStockAvailable => Quantity <= StockQuantity;
}

public class ShoppingCartDto
{
    public IReadOnlyList<ShoppingCartItemDto> Items { get; init; }
        = [];

    public decimal Total => Items.Sum(item => item.Subtotal);

    public int ItemCount => Items.Sum(item => item.Quantity);

    public bool HasStockIssues =>
        Items.Any(item => !item.IsStockAvailable);
}
