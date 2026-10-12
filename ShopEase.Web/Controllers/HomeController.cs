using Microsoft.AspNetCore.Mvc;
using ShopEase.Application.Features.Categories;
using ShopEase.Application.Features.Products;
using ShopEase.Web.ViewModels;

namespace ShopEase.Web.Controllers;

public class HomeController(
    ProductService productService,
    CategoryService categoryService) : Controller
{
    private readonly ProductService _productService = productService;
    private readonly CategoryService _categoryService = categoryService;

    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        var products = await _productService.GetPageAsync(
            search: null,
            page: 1,
            pageSize: 8,
            cancellationToken: cancellationToken);

        var categories = await _categoryService.GetAllAsync(
            cancellationToken);

        var viewModel = new StoreHomeViewModel
        {
            FeaturedProducts = products.Items,
            Categories = categories
        };

        return View(viewModel);
    }
}
