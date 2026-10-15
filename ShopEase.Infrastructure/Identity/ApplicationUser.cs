using Microsoft.AspNetCore.Identity;

namespace ShopEase.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }
}
