using Microsoft.EntityFrameworkCore;
using ShopEase.Application.Interfaces;
using ShopEase.Domain.Entities;

namespace ShopEase.Application.Features.Categories;

public sealed class CategoryService(IShopEaseDbContext db)
{
    private readonly IShopEaseDbContext _db = db;

    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _db.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                ProductCount = _db.Products.Count(p => p.CategoryId == c.Id)
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<CategoryEditDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _db.Categories
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CategoryEditDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<int> CreateAsync(
        CategoryEditDto dto,
        CancellationToken cancellationToken = default)
    {
        var name = dto.Name.Trim();

        var exists = await _db.Categories
            .AnyAsync(c => c.Name == name, cancellationToken);

        if (exists)
            throw new InvalidOperationException("A category with this name already exists.");

        var category = new Category(name);
        category.Update(name, dto.Description?.Trim());

        _db.Categories.Add(category);
        await _db.SaveChangesAsync(cancellationToken);

        return category.Id;
    }

    public async Task<bool> UpdateAsync(
        CategoryEditDto dto,
        CancellationToken cancellationToken = default)
    {
        var category = await _db.Categories
            .FirstOrDefaultAsync(c => c.Id == dto.Id, cancellationToken);

        if (category is null)
            return false;

        var name = dto.Name.Trim();

        var exists = await _db.Categories.AnyAsync(
            c => c.Name == name && c.Id != dto.Id,
            cancellationToken);

        if (exists)
            throw new InvalidOperationException("A category with this name already exists.");

        category.Update(name, dto.Description?.Trim());
        await _db.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var category = await _db.Categories
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (category is null)
            return false;

        var hasProducts = await _db.Products
            .AnyAsync(p => p.CategoryId == id, cancellationToken);

        if (hasProducts)
            throw new InvalidOperationException(
                "This category cannot be deleted because it contains products.");

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync(cancellationToken);

        return true;
    }
}