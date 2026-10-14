using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ShopEase.Domain.Entities;

namespace ShopEase.Application.Interfaces;

public interface IShopEaseDbContext
{
    DbSet<Product> Products { get; }

    DbSet<Category> Categories { get; }

    DbSet<ShoppingCart> ShoppingCarts { get; }

    DbSet<ShoppingCartItem> ShoppingCartItems { get; }

    DbSet<Order> Orders { get; }

    DatabaseFacade Database { get; }

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}