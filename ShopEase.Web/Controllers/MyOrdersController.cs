using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopEase.Application.Services;

namespace ShopEase.Web.Controllers;

[Authorize]
public class MyOrdersController(
    CustomerOrderService customerOrderService) : Controller
{
    private readonly CustomerOrderService _customerOrderService = customerOrderService;

    [HttpGet]
    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        var customerId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(customerId))
            return Challenge();

        var orders = await _customerOrderService.GetMyOrdersAsync(
            customerId, cancellationToken);

        return View(orders);
    }

    [HttpGet]
    public async Task<IActionResult> Details(
        string orderNumber,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
            return NotFound();

        var customerId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(customerId))
            return Challenge();

        var order = await _customerOrderService.GetMyOrderAsync(
            customerId,
            orderNumber,
            cancellationToken);

        // Return 404 for missing orders and orders belonging to
        // another customer, so ownership is not disclosed.
        if (order is null)
            return NotFound();

        return View(order);
    }
}
