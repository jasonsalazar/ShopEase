using Microsoft.AspNetCore.Identity;

namespace ShopEase.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }

    // A stable key for the user's persistent shopping cart.
    public string? CartKey { get; set; }
}
