using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShopEase.Application.Interfaces;
using ShopEase.Infrastructure.Persistence;

namespace ShopEase.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "DefaultConnection is not configured.");

        services.AddDbContext<ShopEaseDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IShopEaseDbContext>(
            provider =>
                provider.GetRequiredService<ShopEaseDbContext>());

        return services;
    }
}