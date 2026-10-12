
using Microsoft.AspNetCore.Mvc;
using ShopEase.Application.Features.Categories;
using ShopEase.Application.Features.Products;
using ShopEase.Web.ViewModels;

namespace ShopEase.Web.Controllers;

public class StoreController(
    ProductService productService,
    CategoryService categoryService) : Controller
{
    private readonly ProductService _productService = productService;
    private readonly CategoryService _categoryService = categoryService;

    public async Task<IActionResult> Index(
        string? search,
        int? categoryId,
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        const int pageSize = 12;

        var products = await _productService.GetPageAsync(
            search: search,
            categoryId: categoryId,
            page: page,
            pageSize: pageSize,
            cancellationToken: cancellationToken);

        var categories = await _categoryService.GetAllAsync(
            cancellationToken);

        var viewModel = new StoreCatalogViewModel
        {
            Products = products,
            Categories = categories
        };

        return View(viewModel);
    }

    public async Task<IActionResult> Details(
        int id,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return NotFound();
        }

        var product = await _productService.GetByIdAsync(
            id,
            cancellationToken);

        if (product is null)
        {
            return NotFound();
        }

        return View(product);
    }

}
