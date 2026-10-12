using Microsoft.EntityFrameworkCore;
using ShopEase.Application.Interfaces;
using ShopEase.Domain.Entities;

namespace ShopEase.Application.Features.Products;

public sealed class ProductService(IShopEaseDbContext db)
{
    private readonly IShopEaseDbContext _db = db;

    public async Task<ProductPageDto> GetPageAsync(
        string? search,
        int? categoryId,
        int page,
        int pageSize = 12,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        search = search?.Trim();

        var query = _db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p => p.Name.Contains(search));
        }

        if (categoryId.HasValue && categoryId.Value > 0)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(p => p.Name)
            .ThenBy(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductListItemDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                CategoryName = p.Category!.Name,
                ImageUrl = p.ImageUrl
            })
            .ToListAsync(cancellationToken);

        return new ProductPageDto
        {
            Items = items,
            Search = search,
            CategoryId = categoryId,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<ProductDetailsDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _db.Products
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new ProductDetailsDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                ImageUrl = p.ImageUrl,
                CategoryId = p.CategoryId,
                CategoryName = p.Category!.Name,
                CreatedAt = p.CreatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<int> CreateAsync(
        ProductEditDto dto,
        CancellationToken cancellationToken = default)
    {
        var categoryExists = await _db.Categories
            .AnyAsync(c => c.Id == dto.CategoryId, cancellationToken);

        if (!categoryExists)
            throw new InvalidOperationException("The selected category does not exist.");

        var product = new Product(
            dto.Name,
            dto.Price,
            dto.StockQuantity,
            dto.CategoryId);

        product.Update(
            dto.Name,
            dto.Description,
            dto.Price,
            dto.StockQuantity,
            dto.ImageUrl,
            dto.CategoryId);

        _db.Products.Add(product);
        await _db.SaveChangesAsync(cancellationToken);

        return product.Id;
    }

    public async Task<bool> UpdateAsync(
        ProductEditDto dto,
        CancellationToken cancellationToken = default)
    {
        var product = await _db.Products
            .FirstOrDefaultAsync(p => p.Id == dto.Id, cancellationToken);

        if (product is null)
            return false;

        var categoryExists = await _db.Categories
            .AnyAsync(c => c.Id == dto.CategoryId, cancellationToken);

        if (!categoryExists)
            throw new InvalidOperationException("The selected category does not exist.");

        product.Update(
            dto.Name,
            dto.Description,
            dto.Price,
            dto.StockQuantity,
            dto.ImageUrl,
            dto.CategoryId);

        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var product = await _db.Products
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (product is null)
            return false;

        _db.Products.Remove(product);
        await _db.SaveChangesAsync(cancellationToken);

        return true;
    }
}