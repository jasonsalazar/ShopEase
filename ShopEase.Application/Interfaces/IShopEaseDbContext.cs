using Microsoft.EntityFrameworkCore;
using ShopEase.Domain.Entities;

namespace ShopEase.Application.Interfaces;

public interface IShopEaseDbContext
{
    DbSet<Product> Products { get; }

    DbSet<Category> Categories { get; }

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}