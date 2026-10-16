using Microsoft.EntityFrameworkCore;
using ShopEase.Application.DTOs;
using ShopEase.Application.Interfaces;
using ShopEase.Domain.Entities;

namespace ShopEase.Application.Services;

public class ShoppingCartService(IShopEaseDbContext db)
{
    private readonly IShopEaseDbContext _db = db;

    private async Task<ShoppingCart?> FindCartAsync(
        string cartKey,
        CancellationToken cancellationToken)
    {
        return await _db.ShoppingCarts
            .Include(cart => cart.Items)
            .SingleOrDefaultAsync(
                cart => cart.CartKey == cartKey,
                cancellationToken);
    }

    private async Task<ShoppingCart> GetOrCreateCartAsync(
        string cartKey,
        CancellationToken cancellationToken)
    {
        var cart = await FindCartAsync(cartKey, cancellationToken);

        if (cart is not null)
            return cart;

        cart = new ShoppingCart(cartKey);
        _db.ShoppingCarts.Add(cart);

        await _db.SaveChangesAsync(cancellationToken);

        return cart;
    }

    public async Task<ShoppingCartDto> GetAsync(
        string cartKey,
        CancellationToken cancellationToken = default)
    {
        var cart = await _db.ShoppingCarts
            .AsNoTracking()
            .Include(cart => cart.Items)
                .ThenInclude(item => item.Product)
                    .ThenInclude(product => product!.Category)
            .SingleOrDefaultAsync(
                cart => cart.CartKey == cartKey,
                cancellationToken);

        if (cart is null)
            return new ShoppingCartDto();

        var items = cart.Items
            .Where(item => item.Product is not null)
            .Select(item => new ShoppingCartItemDto
            {
                ProductId = item.ProductId,
                Name = item.Product!.Name,
                ImageUrl = item.Product.ImageUrl,
                CategoryName = item.Product.Category!.Name,
                UnitPrice = item.Product.Price,
                Quantity = item.Quantity,
                StockQuantity = item.Product.StockQuantity
            })
            .ToList();

        return new ShoppingCartDto { Items = items };
    }

    public async Task AddAsync(
        string cartKey,
        int productId,
        int quantity,
        CancellationToken cancellationToken = default)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive.");

        var product = await _db.Products
            .AsNoTracking()
            .SingleOrDefaultAsync(
                product => product.Id == productId,
                cancellationToken);

        if (product is null)
            throw new InvalidOperationException("Product not found.");

        var cart = await GetOrCreateCartAsync(
            cartKey, cancellationToken);

        var existing = cart.Items.FirstOrDefault(
            item => item.ProductId == productId);

        var requestedQuantity =
            (long)(existing?.Quantity ?? 0) + quantity;

        if (requestedQuantity > product.StockQuantity)
            throw new InvalidOperationException(
                $"Only {product.StockQuantity} item(s) are available.");

        cart.AddItem(productId, quantity);

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        string cartKey,
        int productId,
        int quantity,
        CancellationToken cancellationToken = default)
    {
        var cart = await FindCartAsync(cartKey, cancellationToken);

        if (cart is null)
            throw new InvalidOperationException("Cart not found.");

        if (!cart.Items.Any(item => item.ProductId == productId))
            throw new InvalidOperationException(
                "Product is not in this cart.");

        if (quantity <= 0)
        {
            cart.RemoveItem(productId);
            await _db.SaveChangesAsync(cancellationToken);
            return;
        }

        var product = await _db.Products
            .AsNoTracking()
            .SingleOrDefaultAsync(
                product => product.Id == productId,
                cancellationToken);

        if (product is null)
        {
            cart.RemoveItem(productId);
            await _db.SaveChangesAsync(cancellationToken);
            return;
        }

        if (quantity > product.StockQuantity)
            throw new InvalidOperationException(
                $"Only {product.StockQuantity} item(s) are available.");

        cart.UpdateItem(productId, quantity);

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAsync(
        string cartKey,
        int productId,
        CancellationToken cancellationToken = default)
    {
        var cart = await FindCartAsync(cartKey, cancellationToken);

        if (cart is null)
            return;

        cart.RemoveItem(productId);

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task MergeAsync(
    string guestCartKey,
    string customerCartKey,
    CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(guestCartKey) ||
            string.IsNullOrWhiteSpace(customerCartKey))
        {
            throw new ArgumentException("Cart keys are required.");
        }

        if (guestCartKey == customerCartKey)
            return;

        await using var transaction =
            await _db.Database.BeginTransactionAsync(cancellationToken);

        var guestCart = await _db.ShoppingCarts
            .Include(cart => cart.Items)
            .SingleOrDefaultAsync(
                cart => cart.CartKey == guestCartKey,
                cancellationToken);

        if (guestCart is null || guestCart.Items.Count == 0)
            return;

        var customerCart = await _db.ShoppingCarts
            .Include(cart => cart.Items)
            .SingleOrDefaultAsync(
                cart => cart.CartKey == customerCartKey,
                cancellationToken);

        if (customerCart is null)
        {
            customerCart = new ShoppingCart(customerCartKey);
            _db.ShoppingCarts.Add(customerCart);
            await _db.SaveChangesAsync(cancellationToken);
        }

        var guestItems = guestCart.Items.ToList();

        var productIds = guestItems
            .Select(item => item.ProductId)
            .Distinct()
            .ToList();

        var products = await _db.Products
            .Where(product => productIds.Contains(product.Id))
            .ToDictionaryAsync(product => product.Id, cancellationToken);

        foreach (var guestItem in guestItems)
        {
            if (!products.TryGetValue(guestItem.ProductId, out var product))
                continue;

            var currentItem = customerCart.Items.FirstOrDefault(
                item => item.ProductId == guestItem.ProductId);

            var existingQuantity = currentItem?.Quantity ?? 0;

            // Keep the merged quantity within current stock.
            var remainingStock = Math.Max(
                0, product.StockQuantity - existingQuantity);

            var quantityToAdd = Math.Min(
                guestItem.Quantity, remainingStock);

            if (quantityToAdd > 0)
                customerCart.AddItem(product.Id, quantityToAdd);
        }

        guestCart.Clear();

        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
