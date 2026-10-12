using System.ComponentModel.DataAnnotations;

namespace ShopEase.Application.Features.Categories;

public sealed class CategoryDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int ProductCount { get; init; }
}

public sealed class CategoryEditDto
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }
}