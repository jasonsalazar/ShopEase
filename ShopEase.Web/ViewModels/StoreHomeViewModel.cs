using ShopEase.Application.Features.Categories;
using ShopEase.Application.Features.Products;

namespace ShopEase.Web.ViewModels;

public class StoreHomeViewModel
{
    public IReadOnlyList<ProductListItemDto> FeaturedProducts { get; init; }
        = [];

    public IReadOnlyList<CategoryDto> Categories { get; init; }
        = [];
}
