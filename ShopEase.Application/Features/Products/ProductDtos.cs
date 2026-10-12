using System.ComponentModel.DataAnnotations;

namespace ShopEase.Application.Features.Products;

public sealed class ProductListItemDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public int StockQuantity { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
}

public sealed class ProductDetailsDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal Price { get; init; }
    public int StockQuantity { get; init; }
    public string? ImageUrl { get; init; }
    public int CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

public sealed class ProductEditDto
{
    public int Id { get; set; }

    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Range(typeof(decimal), "0.01", "999999999")]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }

    [StringLength(500)]
    public string? ImageUrl { get; set; }

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }
}

public sealed class ProductPageDto
{
    public IReadOnlyList<ProductListItemDto> Items { get; init; }
        = [];

    public string? Search { get; init; }
    public int? CategoryId { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }

    public int TotalPages =>
        (int)Math.Ceiling((double)TotalCount / PageSize);
}