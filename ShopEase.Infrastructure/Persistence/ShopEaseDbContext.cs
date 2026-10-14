using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ShopEase.Application.Interfaces;
using ShopEase.Domain.Entities;

namespace ShopEase.Infrastructure.Persistence;

public class ShopEaseDbContext(
    DbContextOptions<ShopEaseDbContext> options)
        : DbContext(options), IShopEaseDbContext
{
    public DbSet<Product> Products => Set<Product>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<ShoppingCart> ShoppingCarts => Set<ShoppingCart>();

    public DbSet<ShoppingCartItem> ShoppingCartItems => Set<ShoppingCartItem>();

    public DbSet<Order> Orders => Set<Order>();

    public override DatabaseFacade Database => base.Database;

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


        modelBuilder.Entity<ShoppingCart>(entity =>
        {
            entity.HasKey(cart => cart.Id);

            entity.Property(cart => cart.CartKey)
                .HasMaxLength(64)
                .IsRequired();

            entity.HasIndex(cart => cart.CartKey)
                .IsUnique();

            entity.HasMany(cart => cart.Items)
                .WithOne()
                .HasForeignKey(item => item.ShoppingCartId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Navigation(cart => cart.Items)
                .HasField("_items")
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<ShoppingCartItem>(entity =>
        {
            entity.HasKey(item => item.Id);

            entity.Property(item => item.Quantity)
                .IsRequired();

            entity.HasIndex(item => new
            {
                item.ShoppingCartId,
                item.ProductId
            }).IsUnique();

            entity.HasOne(item => item.Product)
                .WithMany()
                .HasForeignKey(item => item.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(order => order.Id);

            entity.Property(order => order.OrderNumber)
                .HasMaxLength(40)
                .IsRequired();

            entity.HasIndex(order => order.OrderNumber)
                .IsUnique();

            entity.Property(order => order.CustomerName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(order => order.Email)
                .HasMaxLength(254)
                .IsRequired();

            entity.Property(order => order.Phone)
                .HasMaxLength(30);

            entity.Property(order => order.ShippingAddress)
                .HasMaxLength(250)
                .IsRequired();

            entity.Property(order => order.City)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(order => order.PostalCode)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(order => order.Status)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            entity.HasMany(order => order.Items)
                .WithOne()
                .HasForeignKey(item => item.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Navigation(order => order.Items)
                .HasField("_items")
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(item => item.Id);

            entity.Property(item => item.ProductName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(item => item.UnitPrice)
                .HasPrecision(18, 2)
                .IsRequired();

            entity.Property(item => item.Quantity)
                .IsRequired();

            // Preserve order history if the catalog product is deleted.
            // ProductId remains a historical reference, not a live FK.
        });

    }
}