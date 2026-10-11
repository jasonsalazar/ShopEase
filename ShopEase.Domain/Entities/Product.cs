namespace ShopEase.Domain.Entities;

public class Product
{
    private Product() { }

    public Product(
        string name,
        decimal price,
        int stockQuantity,
        int categoryId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name is required.");

        if (price <= 0)
            throw new ArgumentOutOfRangeException(nameof(price));

        if (stockQuantity < 0)
            throw new ArgumentOutOfRangeException(nameof(stockQuantity));

        if (categoryId <= 0)
            throw new ArgumentOutOfRangeException(nameof(categoryId));

        Name = name.Trim();
        Price = price;
        StockQuantity = stockQuantity;
        CategoryId = categoryId;
        CreatedAt = DateTime.UtcNow;
    }

    public int Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public decimal Price { get; private set; }

    public int StockQuantity { get; private set; }

    public string? ImageUrl { get; private set; }

    public int CategoryId { get; private set; }

    public Category? Category { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public void Update(
        string name,
        string? description,
        decimal price,
        int stockQuantity,
        string? imageUrl,
        int categoryId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name is required.");

        if (price <= 0)
            throw new ArgumentOutOfRangeException(nameof(price));

        if (stockQuantity < 0)
            throw new ArgumentOutOfRangeException(nameof(stockQuantity));

        if (categoryId <= 0)
            throw new ArgumentOutOfRangeException(nameof(categoryId));

        Name = name.Trim();
        Description = description;
        Price = price;
        StockQuantity = stockQuantity;
        ImageUrl = imageUrl;
        CategoryId = categoryId;
    }
}