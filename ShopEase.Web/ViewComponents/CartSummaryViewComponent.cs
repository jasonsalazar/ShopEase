
using Microsoft.AspNetCore.Mvc;
using ShopEase.Application.Services;

namespace ShopEase.Web.ViewComponents;

public class CartSummaryViewComponent(
    ShoppingCartService shoppingCartService) : ViewComponent
{
    private const string CartCookieName = "ShopEase.CartKey";

    private readonly ShoppingCartService _shoppingCartService = shoppingCartService;

    public async Task<IViewComponentResult> InvokeAsync()
    {
        if (!HttpContext.Request.Cookies.TryGetValue(
                CartCookieName, out var cartKey)
            || !Guid.TryParseExact(cartKey, "N", out _))
        {
            return View(0);
        }

        var cart = await _shoppingCartService.GetAsync(cartKey);

        return View(cart.ItemCount);
    }
}
