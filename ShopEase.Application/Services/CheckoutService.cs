using System.Data;
using Microsoft.EntityFrameworkCore;
using ShopEase.Application.DTOs;
using ShopEase.Application.Interfaces;
using ShopEase.Domain.Entities;

namespace ShopEase.Application.Services;

public class CheckoutService(IShopEaseDbContext db)
{
    private readonly IShopEaseDbContext _db = db;

    public async Task<string> PlaceOrderAsync(
        string cartKey,
        CheckoutRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(cartKey))
            throw new ArgumentException("Invalid cart.");

        await using var transaction =
            await _db.Database.BeginTransactionAsync(cancellationToken);

        var cart = await _db.ShoppingCarts
            .Include(cart => cart.Items)
            .SingleOrDefaultAsync(
                cart => cart.CartKey == cartKey,
                cancellationToken);

        if (cart is null || cart.Items.Count == 0)
            throw new InvalidOperationException(
                "Your cart is empty.");

        var cartItems = cart.Items.ToList();
        var productIds = cartItems
            .Select(item => item.ProductId)
            .Distinct()
            .ToList();

        var products = await _db.Products
            .Where(product => productIds.Contains(product.Id))
            .ToDictionaryAsync(
                product => product.Id, cancellationToken);

        foreach (var cartItem in cartItems)
        {
            if (!products.TryGetValue(
                    cartItem.ProductId, out var product))
            {
                throw new InvalidOperationException(
                    "A product in your cart is no longer available. " +
                    "Please review your cart.");
            }

            if (cartItem.Quantity > product.StockQuantity)
            {
                throw new InvalidOperationException(
                    $"{product.Name}: only {product.StockQuantity} " +
                    "item(s) are currently available.");
            }
        }

        var orderNumber =
            $"SE-{Guid.NewGuid():N}".ToUpperInvariant();

        var order = new Order(
            orderNumber,
            request.CustomerName,
            request.Email,
            request.Phone,
            request.ShippingAddress,
            request.City,
            request.PostalCode);

        foreach (var cartItem in cartItems)
        {
            var product = products[cartItem.ProductId];

            order.AddItem(
                product.Id,
                product.Name,
                product.Price,
                cartItem.Quantity);

            product.ReduceStock(cartItem.Quantity);
        }

        _db.Orders.Add(order);
        cart.Clear();

        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return order.OrderNumber;
    }

    public async Task<Order?> GetOrderAsync(
        string orderNumber,
        CancellationToken cancellationToken = default)
    {
        return await _db.Orders
            .AsNoTracking()
            .Include(order => order.Items)
            .SingleOrDefaultAsync(
                order => order.OrderNumber == orderNumber,
                cancellationToken);
    }
}
