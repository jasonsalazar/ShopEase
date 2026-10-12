using Microsoft.AspNetCore.Mvc.Rendering;
using ShopEase.Application.Features.Products;

namespace ShopEase.Web.ViewModels;

public sealed class ProductFormViewModel
{
    public ProductEditDto Product { get; set; } = new();

    public IEnumerable<SelectListItem> Categories { get; set; } = [];
}