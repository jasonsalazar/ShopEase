
using Microsoft.EntityFrameworkCore;
using ShopEase.Application.DTOs;
using ShopEase.Application.Interfaces;
using ShopEase.Domain.Entities;

namespace ShopEase.Application.Services;

public class OrderManagementService(IShopEaseDbContext db)
{
    private readonly IShopEaseDbContext _db = db;

    public async Task<OrderManagementPageDto> GetPageAsync(
        string? search = null,
        string? status = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var query = _db.Orders.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();

            query = query.Where(order =>
                order.OrderNumber.Contains(term) ||
                order.CustomerName.Contains(term) ||
                order.Email.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<OrderStatus>(
                status, true, out var parsedStatus))
        {
            query = query.Where(order => order.Status == parsedStatus);
        }
        else
        {
            status = null;
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var orders = await query
            .OrderByDescending(order => order.CreatedAt)
            .ThenByDescending(order => order.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(order => new OrderListItemDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                CustomerName = order.CustomerName,
                Email = order.Email,
                CreatedAt = order.CreatedAt,
                Status = order.Status.ToString(),
                Total = order.Items.Sum(item =>
                    item.UnitPrice * item.Quantity)
            })
            .ToListAsync(cancellationToken);

        return new OrderManagementPageDto
        {
            Items = orders,
            Search = search,
            Status = status,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<Order?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _db.Orders
            .AsNoTracking()
            .Include(order => order.Items)
            .SingleOrDefaultAsync(
                order => order.Id == id,
                cancellationToken);
    }

    public async Task UpdateStatusAsync(
        int id,
        OrderStatus newStatus,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(newStatus))
        {
            throw new ArgumentException(
                "Invalid order status.", nameof(newStatus));
        }

        var order = await _db.Orders
            .SingleOrDefaultAsync(
                order => order.Id == id,
                cancellationToken);

        if (order is null)
        {
            throw new KeyNotFoundException("Order not found.");
        }

        order.UpdateStatus(newStatus);

        await _db.SaveChangesAsync(cancellationToken);
    }
}
