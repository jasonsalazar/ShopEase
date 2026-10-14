using Microsoft.EntityFrameworkCore;
using ShopEase.Domain.Entities;

namespace ShopEase.Application.Interfaces;

public interface IShopEaseDbContext
{
    DbSet<Product> Products { get; }

    DbSet<Category> Categories { get; }

    DbSet<ShoppingCart> ShoppingCarts { get; }

    DbSet<ShoppingCartItem> ShoppingCartItems { get; }

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}