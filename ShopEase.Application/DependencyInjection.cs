using Microsoft.Extensions.DependencyInjection;
using ShopEase.Application.Features.Categories;
using ShopEase.Application.Features.Products;

namespace ShopEase.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddScoped<ProductService>();
        services.AddScoped<CategoryService>();

        return services;
    }
}