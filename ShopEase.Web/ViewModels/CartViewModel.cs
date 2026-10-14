using ShopEase.Application.Features.Products;

namespace ShopEase.Web.ViewModels;

public class CartLineViewModel
{
    public required ProductDetailsDto Product { get; init; }

    public int Quantity { get; init; }

    public decimal Subtotal => Product.Price * Quantity;

    public bool IsStockAvailable =>
        Product.StockQuantity >= Quantity;
}

public class CartViewModel
{
    public IReadOnlyList<CartLineViewModel> Items { get; init; }
        = [];

    public decimal Total => Items.Sum(item => item.Subtotal);

    public bool HasStockIssues =>
        Items.Any(item => !item.IsStockAvailable);
}
