using ShopEase.Application.Features.Categories;
using ShopEase.Application.Features.Products;

namespace ShopEase.Web.ViewModels;

public class StoreCatalogViewModel
{
    public ProductPageDto Products { get; init; } = new();

    public IReadOnlyList<CategoryDto> Categories { get; init; } = [];
}
