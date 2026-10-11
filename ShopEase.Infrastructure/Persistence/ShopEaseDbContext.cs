using Microsoft.EntityFrameworkCore;
using ShopEase.Application.Interfaces;
using ShopEase.Domain.Entities;

namespace ShopEase.Infrastructure.Persistence;

public class ShopEaseDbContext
    : DbContext, IShopEaseDbContext
{
    public ShopEaseDbContext(
        DbContextOptions<ShopEaseDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();

    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Categories");

            entity.HasKey(c => c.Id);

            entity.Property(c => c.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(c => c.Description)
                .HasMaxLength(500);

            entity.HasIndex(c => c.Name)
                .IsUnique();
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products");

            entity.HasKey(p => p.Id);

            entity.Property(p => p.Name)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(p => p.Description)
                .HasMaxLength(2000);

            entity.Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            entity.Property(p => p.ImageUrl)
                .HasMaxLength(500);

            entity.HasOne(p => p.Category)
                .WithMany()
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}