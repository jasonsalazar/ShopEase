
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using ShopEase.Application.Features.Categories;
using ShopEase.Application.Features.Products;
using ShopEase.Application.Services;
using ShopEase.Web.Models;
using ShopEase.Web.ViewModels;

namespace ShopEase.Web.Controllers;

public class StoreController(
    ProductService productService,
    CategoryService categoryService,
    ShoppingCartService shoppingCartService) : Controller
{
    private readonly ProductService _productService = productService;
    private readonly CategoryService _categoryService = categoryService;
    private readonly ShoppingCartService _shoppingCartService = shoppingCartService;

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

    private const string CartCookieName = "ShopEase.CartKey";

    private string GetCartKey()
    {
        if (Request.Cookies.TryGetValue(CartCookieName, out var key)
            && Guid.TryParseExact(key, "N", out _))
        {
            return key;
        }

        key = Guid.NewGuid().ToString("N");

        Response.Cookies.Append(
            CartCookieName,
            key,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                IsEssential = true,
                Expires = DateTimeOffset.UtcNow.AddDays(30)
            });

        return key;
    }

    [HttpGet]
    public async Task<IActionResult> Cart(
        CancellationToken cancellationToken)
    {
        var cart = await _shoppingCartService.GetAsync(
            GetCartKey(), cancellationToken);

        return View(cart);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToCart(
        int productId,
        int quantity = 1,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _shoppingCartService.AddAsync(
                GetCartKey(), productId, quantity, cancellationToken);

            TempData["CartSuccess"] = "Product added to your cart.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["CartError"] = ex.Message;
        }
        catch (ArgumentException ex)
        {
            TempData["CartError"] = ex.Message;
        }

        return RedirectToAction(nameof(Cart));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateCart(
        int productId,
        int quantity,
        CancellationToken cancellationToken)
    {
        try
        {
            await _shoppingCartService.UpdateAsync(
                GetCartKey(), productId, quantity, cancellationToken);

            TempData["CartSuccess"] = "Cart updated.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["CartError"] = ex.Message;
        }

        return RedirectToAction(nameof(Cart));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveFromCart(
        int productId,
        CancellationToken cancellationToken)
    {
        await _shoppingCartService.RemoveAsync(
            GetCartKey(), productId, cancellationToken);

        TempData["CartSuccess"] = "Item removed from your cart.";

        return RedirectToAction(nameof(Cart));
    }
}
