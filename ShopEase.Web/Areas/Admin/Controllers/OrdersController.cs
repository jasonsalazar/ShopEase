
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopEase.Application.Services;
using ShopEase.Domain.Entities;

namespace ShopEase.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class OrdersController(OrderManagementService orderService) : Controller
{
    private readonly OrderManagementService _orderService = orderService;

    [HttpGet]
    public async Task<IActionResult> Index(
        string? search,
        string? status,
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        var model = await _orderService.GetPageAsync(
            search,
            status,
            page,
            cancellationToken: cancellationToken);

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Details(
        int id,
        CancellationToken cancellationToken)
    {
        var order = await _orderService.GetByIdAsync(
            id, cancellationToken);

        if (order is null)
            return NotFound();

        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(
        int id,
        string status,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<OrderStatus>(
                status, true, out var newStatus) ||
            !Enum.IsDefined(newStatus))
        {
            TempData["OrderError"] = "Please select a valid status.";

            return RedirectToAction(
                nameof(Details), new { id });
        }

        try
        {
            await _orderService.UpdateStatusAsync(
                id, newStatus, cancellationToken);

            TempData["OrderSuccess"] = "Order status updated.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["OrderError"] = ex.Message;
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Details), new { id });
    }
}
