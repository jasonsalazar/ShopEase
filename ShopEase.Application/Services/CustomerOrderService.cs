
using Microsoft.EntityFrameworkCore;
using ShopEase.Application.DTOs;
using ShopEase.Application.Interfaces;
using ShopEase.Domain.Entities;

namespace ShopEase.Application.Services;

public class CustomerOrderService(IShopEaseDbContext db)
{
    private readonly IShopEaseDbContext _db = db;

    public async Task<IReadOnlyList<OrderListItemDto>> GetMyOrdersAsync(
        string customerId,
        CancellationToken cancellationToken = default)
    {
        return await _db.Orders
            .AsNoTracking()
            .Where(order => order.CustomerId == customerId)
            .OrderByDescending(order => order.CreatedAt)
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
    }

    public async Task<Order?> GetMyOrderAsync(
        string customerId,
        string orderNumber,
        CancellationToken cancellationToken = default)
    {
        return await _db.Orders
            .AsNoTracking()
            .Include(order => order.Items)
            .SingleOrDefaultAsync(
                order =>
                    order.CustomerId == customerId &&
                    order.OrderNumber == orderNumber,
                cancellationToken);
    }
}
